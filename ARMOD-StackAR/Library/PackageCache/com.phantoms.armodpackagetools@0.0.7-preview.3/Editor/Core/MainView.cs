using System;
using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class MainView : EditorWindow
    {
        private readonly EditorGuiSplitView twoViewSplitter =
            new EditorGuiSplitView(EditorGuiSplitView.Direction.Horizontal, 0.3f);

        private int stepId;

        private Vector2 contentScrollPos = Vector2.zero;
        private ProjectDataModel projectDataModel = null;

        private ISubView buildSubView;
        private ISubView propertySubView;
        private ISubView contentListSubView;
        private ISubView projectListSubView;


        Rect RightViewArea => new Rect(twoViewSplitter.ResizeHandleRect.x,
            twoViewSplitter.ResizeHandleRect.y + 21,
            position.width,
            position.height);

        private static MainView WINDOW;

        private enum StepNameEnum
        {
            Property,
            Contents,
            Build,
        }

        [MenuItem("Tools/AR-MOD/Package Tools")]
        private static void DisplayPackageToolsWindow()
        {
            WINDOW = GetWindow<MainView>();
            WINDOW.titleContent = new GUIContent("Package Tools");
            WINDOW.minSize = new Vector2(800, 500);
            WINDOW.Show();
        }

        private void PreloadData()
        {
            projectDataModel = AssetDatabase.LoadAssetAtPath<ProjectDataModel>(
                Utility.GetRootDataPath(ConstKey.TOOLS_DATA_CACHE_ASSETS));
            if (projectDataModel)
                projectDataModel.Init();
            else
            {
                projectDataModel = CreateInstance<ProjectDataModel>();
                projectDataModel.Init();

                AssetDatabase.CreateAsset(projectDataModel,
                    Utility.GetRootDataPath(ConstKey.TOOLS_DATA_CACHE_ASSETS));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        #region Create and init sub views

        private void InitProjectListView()
        {
            if (projectListSubView != null || projectDataModel == null) return;
            projectListSubView = new ProjectListSubView(projectDataModel.ProjectTreeElements,
                OnProjectOpen, OnProjectRemoved, OnProjectClosed);
        }


        private void InitPropertyView()
        {
            if (propertySubView != null || projectDataModel == null) return;
            propertySubView = new DrawPropertyView(projectDataModel);
        }

        private void InitBuildView()
        {
            if (buildSubView != null || projectDataModel == null) return;
            buildSubView = new DrawBuildView(projectDataModel);
        }


        private void InitContentListView()
        {
            if (contentListSubView != null || projectDataModel == null) return;
            contentListSubView = new ContentListSubView(projectDataModel);
        }

        #endregion

        #region Project oeration event

        private void OnProjectClosed(int _id)
        {
            projectDataModel.CurrentOpenProjectId = -1;
            projectDataModel.CurrentContentViewId = 0;
            (contentListSubView as ContentListSubView)?.EnableDragAndDrop(false);
            (contentListSubView as ContentListSubView)?.ReloadTreeView();
            (projectListSubView as ProjectListSubView)?.CloseProject(_id);
            propertySubView = null;
            contentListSubView = null;
        }


        private void OnProjectRemoved(int _id)
        {
            if (projectDataModel.CurrentOpenProjectId.Equals(_id))
            {
                if (EditorUtility.DisplayDialog("Error!", "Your project is opening! You can not to remove it!",
                    "OK"))
                    return;
            }

            (contentListSubView as ContentListSubView)?.EnableDragAndDrop(false);
            (contentListSubView as ContentListSubView)?.ReloadTreeView();
            (projectListSubView as ProjectListSubView)?.CloseProject(_id);
            (projectListSubView as ProjectListSubView)?.RemoveProject();


            //re-sort all id
            for (int tmp_Index = 1; tmp_Index < projectDataModel.ProjectTreeElements.Count; tmp_Index++)
            {
                var tmp_Project = projectDataModel.ProjectTreeElements[tmp_Index];
                tmp_Project.Id = tmp_Index - 1;
                if (!tmp_Project.IsOpened) continue;
                projectDataModel.CurrentOpenProjectId = tmp_Project.Id;
                projectDataModel.CurrentContentViewId = tmp_Project.Id - 1;
            }
        }

        private void OnProjectOpen(int _id)
        {
            //Close operation When the id equals 0 
            //Now is opening,So we need close it before opening
            if (projectDataModel.CurrentOpenProjectId > -1)
            {
                OnProjectClosed(projectDataModel.CurrentOpenProjectId);
            }


            //refresh the view and data
            projectDataModel.CurrentOpenProjectId = _id;
            projectDataModel.CurrentContentViewId = _id + 1;
            (projectListSubView as ProjectListSubView)?.OpenProject(_id);
            (contentListSubView as ContentListSubView)?.EnableDragAndDrop(true);
            (contentListSubView as ContentListSubView)?.ReloadTreeView();
        }

        #endregion

        #region Editor Methods

        private void OnGUI()
        {
            if (projectDataModel == null || projectDataModel.ProjectTreeElements == null)
            {
                PreloadData();
            }

            twoViewSplitter.BeginSplitView();
            DrawLeftView();
            twoViewSplitter.Split();
            DrawRightView();
            twoViewSplitter.EndSplitView();

            if (Event.current.type == EventType.Repaint)
                Repaint();
        }

        private void DrawLeftView()
        {
            projectListSubView?.DrawSubView(twoViewSplitter.FirstViewRect);
        }

        private void DrawRightView()
        {
            var tmp_ProjectTreeElement = projectDataModel.ProjectTreeElements.Find((_project) => _project.IsOpened);

            if (tmp_ProjectTreeElement == null || !tmp_ProjectTreeElement.IsOpened)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox(" Please open the project you want to edit ", MessageType.Info);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                return;
            }


            EditorGUILayout.BeginVertical();
            stepId = GUILayout.Toolbar(stepId, Enum.GetNames(typeof(StepNameEnum)));

            switch ((StepNameEnum) stepId)
            {
                case StepNameEnum.Property:
                    InitPropertyView();
                    propertySubView?.DrawSubView(RightViewArea);
                    break;
                case StepNameEnum.Contents:
                    InitContentListView();
                    contentListSubView?.DrawSubView(RightViewArea);
                    break;
                case StepNameEnum.Build:
                    InitBuildView();
                    buildSubView?.DrawSubView(RightViewArea);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUILayout.EndVertical();
        }


        private void OnEnable()
        {
            PreloadData();
            InitProjectListView();
        }

        private void OnDisable()
        {
            if (projectDataModel)
            {
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(projectDataModel);
            }

            projectListSubView?.Dispose();
            propertySubView?.Dispose();
            contentListSubView?.Dispose();
            buildSubView?.Dispose();
        }

        #endregion


        public static void ShowNotify(string _msg)
        {
            if (WINDOW)
            {
                EditorUtility.DisplayDialog("Error!", _msg, "Ok");
            }
            else
            {
                DisplayPackageToolsWindow();
                EditorUtility.DisplayDialog("Error!", _msg, "Ok");

            }
        }
    }
}