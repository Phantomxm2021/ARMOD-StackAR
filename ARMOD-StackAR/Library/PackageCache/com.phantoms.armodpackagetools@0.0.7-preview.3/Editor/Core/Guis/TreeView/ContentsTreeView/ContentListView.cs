using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ContentListView : TreeViewWithTreeModel<ContentListElementModel>
    {
        const float KRowHeights = 20f;
        const float KToggleWidth = 18f;
        public bool CanDragAndDrop = false;
        public Action addedNewElement;
        public Action removedElement;

        // All columns
        enum ColumnsType
        {
            Icon1,
            Name,
            AssetPath,
        }

        private enum SortOption
        {
            Name,
            Value1,
            Value2,
        }

        // Sort options per column
        SortOption[] SortOptions =
        {
            SortOption.Value1,
            SortOption.Value2,
            SortOption.Name
        };


        public ContentListView(TreeViewState _state, MultiColumnHeader _multiColumnHeader,
            TreeModel<ContentListElementModel> _model) : base(_state, _multiColumnHeader, _model)
        {
            Assert.AreEqual(SortOptions.Length, Enum.GetValues(typeof(ColumnsType)).Length,
                "Ensure number of sort options are in sync with number of ColumnsType enum values");
            rowHeight = KRowHeights;
            columnIndexForTreeFoldouts = 2;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            // center foldout in the row since we also center content. See RowGUI
            customFoldoutYOffset = (KRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = KToggleWidth;
            _multiColumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }

        // Note we We only build the visible rows, only the backend has the full tree information. 
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem _root)
        {
            return base.BuildRows(_root);
        }


        protected override void RowGUI(RowGUIArgs _args)
        {
            var tmp_Item = (TreeViewItem<ContentListElementModel>) _args.item;

            for (int tmp_I = 0; tmp_I < _args.GetNumVisibleColumns(); ++tmp_I)
            {
                DrawCellGui(_args.GetCellRect(tmp_I), tmp_Item, (ColumnsType) _args.GetColumn(tmp_I), ref _args);
            }
        }

        public override void OnGUI(Rect _rect)
        {
            base.OnGUI(_rect);
        }

        protected override bool CanStartDrag(CanStartDragArgs _args)
        {
            return base.CanStartDrag(_args);
        }


        protected override void SetupDragAndDrop(SetupDragAndDropArgs _args)
        {
            base.SetupDragAndDrop(_args);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs _args)
        {
            if (!CanDragAndDrop) return DragAndDropVisualMode.None;
            if (!_args.performDrop) return DragAndDropVisualMode.Link;
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            DragAndDrop.AcceptDrag();

            var tmp_Root = GetTreeModel.GetRoot;

            foreach (Object tmp_DraggedObject in DragAndDrop.objectReferences)
            {
                if (tmp_DraggedObject is DefaultAsset)
                {
                    continue;
                }

                string tmp_AssetPath = AssetDatabase.GetAssetPath(tmp_DraggedObject);
                int tmp_ListCount = tmp_Root.HasChildren ? tmp_Root.Children.Count : 0;
                var tmp_NewElement =
                    new ContentListElementModel(tmp_DraggedObject.name, tmp_AssetPath, 0, tmp_ListCount,
                        tmp_DraggedObject.GetInstanceID());
                GetTreeModel.AddElement(tmp_NewElement, tmp_Root, tmp_ListCount);
                addedNewElement?.Invoke();
                
            }

            return DragAndDropVisualMode.Link;
        }


        protected override void ContextClickedItem(int _id)
        {
            base.ContextClickedItem(_id);
            var tmp_Item = GetTreeModel.Find(_id);
            GenericMenu tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("Ping Object"), false, () =>
            {
                var tmp_Object = AssetDatabase.LoadAssetAtPath(tmp_Item.AssetPath, typeof(Object));
                EditorGUIUtility.PingObject(tmp_Object);
            });
            tmp_Menu.AddItem(new GUIContent("Short Name"), false, () =>
            {
                tmp_Item.Name = Path.GetFileNameWithoutExtension(tmp_Item.Name);
                RenameEnded(new RenameEndedArgs
                {
                    newName = Path.GetFileNameWithoutExtension(tmp_Item.Name), itemID = tmp_Item.Id,
                    acceptedRename = true
                });
            });
            tmp_Menu.AddItem(new GUIContent("Rename"), false, () => { BeginRename(FindItem(_id, rootItem)); });
            tmp_Menu.AddSeparator("");
            tmp_Menu.AddItem(new GUIContent("Remove"), false, () =>
            {
                GetTreeModel.RemoveElements(GetSelection());
                removedElement?.Invoke();
                var tmp_Children = GetTreeModel.GetRoot.Children;
                for (int tmp_Index = 0; tmp_Index < tmp_Children.Count; tmp_Index++)
                {
                    tmp_Children[tmp_Index].Id = tmp_Index;
                }
            });

            tmp_Menu.ShowAsContext();
        }


        void OnSortingChanged(MultiColumnHeader _multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void DrawCellGui(Rect _cellRect, TreeViewItem<ContentListElementModel> _item, ColumnsType _column,
            ref RowGUIArgs _args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref _cellRect);

            switch (_column)
            {
                case ColumnsType.Name:
                    _args.rowRect = _cellRect;
                    base.RowGUI(_args);
                    break;
                case ColumnsType.AssetPath:
                    DefaultGUI.Label(_cellRect, _item.data.AssetPath, _args.selected, _args.focused);
                    break;
            }
        }


        void SortIfNeeded(TreeViewItem _root, IList<TreeViewItem> _rows)
        {
            if (_rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(_root, _rows);
            Repaint();
        }

        void SortByMultipleColumns()
        {
            var tmp_SortedColumns = multiColumnHeader.state.sortedColumns;

            if (tmp_SortedColumns.Length == 0)
                return;

            var tmp_MyTypes = rootItem.children.Cast<TreeViewItem<ContentListElementModel>>();
            var tmp_OrderedQuery = InitialOrder(tmp_MyTypes, tmp_SortedColumns);
            for (int tmp_I = 1; tmp_I < tmp_SortedColumns.Length; tmp_I++)
            {
                SortOption tmp_SortOption = SortOptions[tmp_SortedColumns[tmp_I]];
                bool tmp_Ascending = multiColumnHeader.IsSortedAscending(tmp_SortedColumns[tmp_I]);

                switch (tmp_SortOption)
                {
                    case SortOption.Name:
                        tmp_OrderedQuery = tmp_OrderedQuery.ThenBy(_l => _l.data.Name, tmp_Ascending);
                        break;
                }
            }

            rootItem.children = tmp_OrderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<ContentListElementModel>> InitialOrder(
            IEnumerable<TreeViewItem<ContentListElementModel>> _myTypes,
            int[] _history)
        {
            SortOption tmp_SortOption = SortOptions[_history[0]];
            bool tmp_Ascending = multiColumnHeader.IsSortedAscending(_history[0]);
            switch (tmp_SortOption)
            {
                case SortOption.Name:
                    return _myTypes.Order(_l => _l.data.Name, tmp_Ascending);
            }

            // default
            return _myTypes.Order(_l => _l.data.Name, tmp_Ascending);
        }

        private static void TreeToList(TreeViewItem _root, IList<TreeViewItem> _result)
        {
            if (_root == null)
                throw new NullReferenceException("GetRoot");
            if (_result == null)
                throw new NullReferenceException("result");

            _result.Clear();

            if (_root.children == null)
                return;

            Stack<TreeViewItem> tmp_Stack = new Stack<TreeViewItem>();
            for (int tmp_I = _root.children.Count - 1; tmp_I >= 0; tmp_I--)
                tmp_Stack.Push(_root.children[tmp_I]);

            while (tmp_Stack.Count > 0)
            {
                TreeViewItem tmp_Current = tmp_Stack.Pop();
                _result.Add(tmp_Current);

                if (tmp_Current.hasChildren && tmp_Current.children[0] != null)
                {
                    for (int tmp_I = tmp_Current.children.Count - 1; tmp_I >= 0; tmp_I--)
                    {
                        tmp_Stack.Push(tmp_Current.children[tmp_I]);
                    }
                }
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float _treeViewWidth)
        {
            var tmp_Columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel"), ""),
                    contextMenuText = "Asset",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 200,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Path", ""),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 110,
                    minWidth = 60,
                    autoResize = true
                }
            };

            Assert.AreEqual(tmp_Columns.Length, Enum.GetValues(typeof(ColumnsType)).Length,
                "Number of columns should match number of enum values: You probably forgot to update one of them.");
            return new MultiColumnHeaderState(tmp_Columns);
        }
    }

    static class MyExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> _source, Func<T, TKey> _selector,
            bool _ascending)
        {
            if (_ascending)
            {
                return _source.OrderBy(_selector);
            }
            else
            {
                return _source.OrderByDescending(_selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> _source, Func<T, TKey> _selector,
            bool _ascending)
        {
            if (_ascending)
            {
                return _source.ThenBy(_selector);
            }
            else
            {
                return _source.ThenByDescending(_selector);
            }
        }
    }
}