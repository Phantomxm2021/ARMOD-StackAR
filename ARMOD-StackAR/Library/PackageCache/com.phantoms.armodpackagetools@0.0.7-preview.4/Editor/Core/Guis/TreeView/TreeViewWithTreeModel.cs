using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace  com.Phantoms.ARMODPackageTools.Core
{
    public class TreeViewWithTreeModel<T> : TreeView where T : TreeElementModel
    {
        readonly List<TreeViewItem> rows = new List<TreeViewItem>();
        public event Action TreeChanged;
        public event Action<IList<TreeViewItem>> BeforeDroppingDraggedItems;
        public TreeModel<T> GetTreeModel { get; private set; }

        public TreeViewWithTreeModel(TreeViewState _state, TreeModel<T> _model) : base(_state)
        {
            Init(_model);
        }

        protected TreeViewWithTreeModel(TreeViewState _state, MultiColumnHeader _multiColumnHeader, TreeModel<T> _model)
            : base(_state, _multiColumnHeader)
        {
            Init(_model);
        }

        void Init(TreeModel<T> _model)
        {
            GetTreeModel = _model;
            GetTreeModel.ModelChanged += ModelChanged;
        }

        void ModelChanged()
        {
            TreeChanged?.Invoke();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            int depthForHiddenRoot = -1;
            return new TreeViewItem<T>(GetTreeModel.GetRoot.Id, depthForHiddenRoot, GetTreeModel.GetRoot.Name,
                GetTreeModel.GetRoot);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem _root)
        {
            if (GetTreeModel.GetRoot == null)
            {
                Debug.LogError("tree model GetRoot is null. did you call SetData()?");
            }

            rows.Clear();
            if (!string.IsNullOrEmpty(searchString))
            {
                Search(GetTreeModel.GetRoot, searchString, rows);
            }
            else
            {
                if (GetTreeModel.GetRoot != null && GetTreeModel.GetRoot.HasChildren)
                    AddChildrenRecursive(GetTreeModel.GetRoot, 0, rows);
            }

            // We still need to setup the child parent information for the _rows since this 
            // information is used by the TreeView internal logic (navigation, dragging etc)
            SetupParentsAndChildrenFromDepths(_root, rows);

            return rows;
        }

        void AddChildrenRecursive(T _parent, int _depth, IList<TreeViewItem> _newRows)
        {
            foreach (var tmp_TreeElement in _parent.Children)
            {
                var tmp_Child = (T) tmp_TreeElement;
                var tmp_Item = new TreeViewItem<T>(tmp_Child.Id, _depth, tmp_Child.Name, tmp_Child);
                _newRows.Add(tmp_Item);

                if (!tmp_Child.HasChildren) continue;
                if (IsExpanded(tmp_Child.Id))
                {
                    AddChildrenRecursive(tmp_Child, _depth + 1, _newRows);
                }
                else
                {
                    tmp_Item.children = CreateChildListForCollapsedParent();
                }
            }
        }

        protected override bool CanRename(TreeViewItem _item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs _args)
        {
            base.RenameEnded(_args);
            if (!_args.acceptedRename) return;
            GetTreeModel.Find(_args.itemID).Name = _args.newName;
            Reload();
        }


        private void Search(T _searchFromThis, string _search, List<TreeViewItem> _result)
        {
            if (string.IsNullOrEmpty(_search))
                throw new ArgumentException("Invalid search: cannot be null or empty", nameof(_search));

            const int kItemDepth = 0; // tree is flattened when searching

            Stack<T> tmp_Stack = new Stack<T>();
            foreach (var tmp_Element in _searchFromThis.Children)
                tmp_Stack.Push((T) tmp_Element);
            while (tmp_Stack.Count > 0)
            {
                T tmp_Current = tmp_Stack.Pop();
                // Matches search?
                if (tmp_Current.Name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _result.Add(new TreeViewItem<T>(tmp_Current.Id, kItemDepth, tmp_Current.Name, tmp_Current));
                }

                if (tmp_Current.Children == null || tmp_Current.Children.Count <= 0) continue;
                foreach (var tmp_Element in tmp_Current.Children)
                {
                    tmp_Stack.Push((T) tmp_Element);
                }
            }

            SortSearchResult(_result);
        }

        protected virtual void SortSearchResult(List<TreeViewItem> _rows)
        {
            _rows.Sort((_x, _y) =>
                EditorUtility.NaturalCompare(_x.displayName,
                    _y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
        }

        protected override IList<int> GetAncestors(int _id)
        {
            return GetTreeModel.GetAncestors(_id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int _id)
        {
            return GetTreeModel.GetDescendantsThatHaveChildren(_id);
        }


        // Dragging
        //-----------

        private const string CONST_K_GENERIC_DRAG_ID = "GenericDragColumnDragging";

        protected override bool CanStartDrag(CanStartDragArgs _args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs _args)
        {
            if (hasSearch)
                return;

            DragAndDrop.PrepareStartDrag();
            var tmp_DraggedRows = GetRows().Where(_item => _args.draggedItemIDs.Contains(_item.id)).ToList();
            DragAndDrop.SetGenericData(CONST_K_GENERIC_DRAG_ID, tmp_DraggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
            string tmp_Title = tmp_DraggedRows.Count == 1 ? tmp_DraggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(tmp_Title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs _args)
        {
            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            if (!(DragAndDrop.GetGenericData(CONST_K_GENERIC_DRAG_ID) is List<TreeViewItem> tmp_DraggedRows))
                return DragAndDropVisualMode.None;

            // Parent item is null when dragging outside any tree view items.
            switch (_args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                {
                    bool tmp_ValidDrag = ValidDrag(_args.parentItem, tmp_DraggedRows);
                    if (_args.performDrop && tmp_ValidDrag)
                    {
                        T tmp_ParentData = ((TreeViewItem<T>) _args.parentItem).data;
                        OnDropDraggedElementsAtIndex(tmp_DraggedRows, tmp_ParentData,
                            _args.insertAtIndex == -1 ? 0 : _args.insertAtIndex);
                    }

                    return tmp_ValidDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                }

                case DragAndDropPosition.OutsideItems:
                {
                    if (_args.performDrop)
                        OnDropDraggedElementsAtIndex(tmp_DraggedRows, GetTreeModel.GetRoot, GetTreeModel.GetRoot.Children.Count);

                    return DragAndDropVisualMode.Move;
                }

                default:
                    Debug.LogError("Unhandled enum " + _args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        protected virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem> _draggedRows, T _parent, int _insertIndex)
        {
            BeforeDroppingDraggedItems?.Invoke(_draggedRows);

            var tmp_DraggedElements = new List<TreeElementModel>();
            foreach (var tmp_X in _draggedRows)
                tmp_DraggedElements.Add(((TreeViewItem<T>) tmp_X).data);

            var tmp_SelectedIDs = tmp_DraggedElements.Select(_x => _x.Id).ToArray();
            GetTreeModel.MoveElements(_parent, _insertIndex, tmp_DraggedElements);
            SetSelection(tmp_SelectedIDs, TreeViewSelectionOptions.RevealAndFrame);
        }


        bool ValidDrag(TreeViewItem _parent, List<TreeViewItem> _draggedItems)
        {
            TreeViewItem tmp_CurrentParent = _parent;
            while (tmp_CurrentParent != null)
            {
                if (_draggedItems.Contains(tmp_CurrentParent))
                    return false;
                tmp_CurrentParent = tmp_CurrentParent.parent;
            }

            return true;
        }
    }
}