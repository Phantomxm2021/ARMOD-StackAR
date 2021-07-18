using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ContentListSubView : ISubView
    {
        private readonly List<ContentListElementModel> contentTreeElementModelsCache;
        private readonly ProjectDataModel projectDataModel;

        private MultiColumnHeaderState multiColumnHeaderState;
        private TreeViewState treeViewState;
        private ContentListView listView;


        private bool initialized;

        public ContentListSubView(ProjectDataModel _projectDataModel)
        {
            projectDataModel = _projectDataModel;
            contentTreeElementModelsCache = projectDataModel.GetOpeningProject().GetContentTreeElements;
        }


        private void InitIfNeeded(float _multiColumnTreeViewWidth)
        {
            if (initialized) return;
            // Check if it already exists (deserialized from window layout file or scriptable object)
            if (treeViewState == null)
                treeViewState = new TreeViewState();


            var tmp_FirstInit = multiColumnHeaderState == null;
            var tmp_HeaderState =
                ContentListView.CreateDefaultMultiColumnHeaderState(_multiColumnTreeViewWidth);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, tmp_HeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, tmp_HeaderState);
            multiColumnHeaderState = tmp_HeaderState;

            var tmp_MultiColumnHeader = new ContentListHeader(tmp_HeaderState);
            if (tmp_FirstInit)
                tmp_MultiColumnHeader.ResizeToFit();

            var tmp_ContentList = projectDataModel.GetOpeningProject().GetContentTreeElements;
            var tmp_TreeModel = new TreeModel<ContentListElementModel>(tmp_ContentList);
            listView = new ContentListView(treeViewState, tmp_MultiColumnHeader, tmp_TreeModel);
            listView.removedElement += RemovedElement;
            listView.addedNewElement += AddedElement;
            FixResourcePath();

            initialized = true;
        }

        private void RemovedElement()
        {
            Utility.SaveProjectConfigCache(projectDataModel.GetOpeningProject(),
                Path.Combine(projectDataModel.GetOpeningProject().GetProjectPath(), "AutomaticGenerated"));
        }
        
        private void AddedElement()
        {
            Utility.SaveProjectConfigCache(projectDataModel.GetOpeningProject(),
                Path.Combine(projectDataModel.GetOpeningProject().GetProjectPath(), "AutomaticGenerated"));
        }

        internal void ReloadTreeView()
        {
            if (contentTreeElementModelsCache != null)
                listView?.GetTreeModel.SetData(contentTreeElementModelsCache);
            listView?.Reload();
        }

        private void CleanUpElement()
        {
            if (listView?.GetRows() == null) return;
            IList<int> tmp_AllItemId = new List<int>();
            foreach (var tmp_ViewItem in listView.GetRows())
            {
                tmp_AllItemId.Add(tmp_ViewItem.id);
            }

            listView.GetTreeModel.RemoveElements(tmp_AllItemId);
        }

        internal void EnableDragAndDrop(bool _enable)
        {
            if (listView != null)
                listView.CanDragAndDrop = _enable;
        }

        public void DrawSubView(Rect _area)
        {
            InitIfNeeded(_area.width);
            listView.OnGUI(_area);
        }

        public void Dispose()
        {
        }

        private void FixResourcePath()
        {
            foreach (var tmp_Element in projectDataModel.ProjectTreeElements)
            {
                if (tmp_Element.IsOpened)
                    EnableDragAndDrop(true);

                for (var tmp_Index = 0; tmp_Index < tmp_Element.GetContentCount; tmp_Index++)
                {
                    var tmp_ElementData = tmp_Element.GetContentTreeElements[tmp_Index];
                    var tmp_AssetName = tmp_ElementData.AssetName;
                    if (string.IsNullOrEmpty(tmp_AssetName) || tmp_AssetName.Equals("Root")) continue;

                    if (AssetDatabase.LoadAssetAtPath(tmp_ElementData.AssetPath, typeof(Object))) continue;
                    var tmp_InstanceId = tmp_ElementData.InstanceId;
                    var tmp_ChangePath = AssetDatabase.GetAssetPath(tmp_InstanceId);
                    if (!string.IsNullOrEmpty(tmp_ChangePath))
                    {
                        Debug.Log(
                            $"Your are changed '{tmp_AssetName}' asset path!{tmp_ElementData.AssetPath} to {tmp_ChangePath}");
                        tmp_ElementData.AssetPath = tmp_ChangePath;
                    }
                    else
                    {
                        tmp_Element.RemoveElementFromList(tmp_Element.GetContentTreeElements, tmp_Index);
                        ReloadTreeView();
                        Debug.LogError($"We were removed asset{tmp_AssetName}");
                    }
                }
            }
        }
    }
}