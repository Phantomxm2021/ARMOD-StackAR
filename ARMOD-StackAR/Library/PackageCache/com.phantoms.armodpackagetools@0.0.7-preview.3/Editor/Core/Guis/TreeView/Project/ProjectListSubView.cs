using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ProjectListSubView : ISubView
    {
        bool initialized;

        // Serialized in the window layout file so it survives assembly reloading
        TreeViewState treeViewState;
        ProjectListView listView;
        readonly List<ProjectListElementModel> projectTreeAsset;
        private MultiColumnHeaderState multiColumnHeaderState;
        private SearchField searchField;
        private Rect position;
        private readonly Action<int> onProjectOpen;
        private readonly Action<int> onProjectRemoved;
        private readonly Action<int> onProjectClosed;
        Rect ToolbarRect => new Rect(0, 10, position.width, 20f);


        public ProjectListSubView(List<ProjectListElementModel> _projectTreeAsset,
            Action<int> _onProjectOpen,
            Action<int> _onProjectRemoved,
            Action<int> _onProjectClosed)
        {
            projectTreeAsset = _projectTreeAsset;
            onProjectOpen = _onProjectOpen;
            onProjectRemoved = _onProjectRemoved;
            onProjectClosed = _onProjectClosed;
        }

        void InitIfNeeded(float _multiColumnTreeViewWidth)
        {
            if (Math.Abs(_multiColumnTreeViewWidth) <= 0) return;
            if (initialized) return;
            bool tmp_FirstInit = multiColumnHeaderState == null;
            var tmp_HeaderState =
                ProjectListView.CreateDefaultMultiColumnHeaderState(_multiColumnTreeViewWidth);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, tmp_HeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, tmp_HeaderState);
            multiColumnHeaderState = tmp_HeaderState;

            var tmp_MultiColumnHeader = new ContentListHeader(tmp_HeaderState);
            if (tmp_FirstInit)
                tmp_MultiColumnHeader.ResizeToFit();

            // Check if it already exists (deserialized from window layout file or scriptable object)
            if (treeViewState == null)
                treeViewState = new TreeViewState();
            var tmp_TreeModel = new TreeModel<ProjectListElementModel>(projectTreeAsset);
            listView = new ProjectListView(treeViewState, tmp_MultiColumnHeader, tmp_TreeModel);

            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += listView.SetFocusAndEnsureSelectedItem;
            ProjectListView.OnProjectOpen += onProjectOpen;
            ProjectListView.OnProjectRemoved += onProjectRemoved;
            ProjectListView.OnProjectClosed += onProjectClosed;
            initialized = true;
        }

        public void CloseProject(int _id)
        {
            listView.GetTreeModel.Find(_id).IsOpened = false;
        }


        public void OpenProject(int _id)
        {
            var tmp_ProjectListElementModel = listView.GetTreeModel.Find(_id);
            tmp_ProjectListElementModel.IsOpened = true;

            if (!tmp_ProjectListElementModel.Configures)
            {
                var tmp_ConfigureFilePath =
                    Path.Combine(Utility.ShortenPath(tmp_ProjectListElementModel.GetProjectPath()),
                        "Configures/Configure.asset");
                tmp_ProjectListElementModel.Configures =
                    AssetDatabase.LoadAssetAtPath<Configures>(tmp_ConfigureFilePath);
            }

            tmp_ProjectListElementModel.Configures.ProjectName = projectTreeAsset[_id + 1].Name;
        }

        public void RemoveProject()
        {
            if (listView == null) return;
            IList<int> tmp_SelectedIds = listView.GetSelection();
            listView.GetTreeModel.RemoveElements(tmp_SelectedIds);
        }

        private void DrawSearchBar(Rect _rect)
        {
            listView.searchString = searchField.OnGUI(_rect, listView.searchString);
        }

        public void DrawSubView(Rect _area)
        {
            position = _area;
            InitIfNeeded(_area.width);
            if (!initialized) return;
            DrawSearchBar(ToolbarRect);
            listView.OnGUI(new Rect(_area.x, _area.y + 30, _area.width, _area.height));
        }

        public void Dispose()
        {
        }
    }
}