using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;


namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ProjectListView : TreeViewWithTreeModel<ProjectListElementModel>
    {
        const float K_ROW_HEIGHTS = 20f;
        private bool clickedOnItem;
        public static event Action<int> OnProjectOpen;
        public static event Action<int> OnProjectRemoved;
        public static event Action<int> OnProjectClosed;

        enum ColumnsType
        {
            Name,
        }

        public ProjectListView(TreeViewState _state, MultiColumnHeader _multiColumnHeader,
            TreeModel<ProjectListElementModel> _model) : base(_state, _multiColumnHeader, _model)
        {
            showBorder = true;
            rowHeight = K_ROW_HEIGHTS;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            // center foldout in the row since we also center content. See RowGUI
            customFoldoutYOffset = (K_ROW_HEIGHTS - EditorGUIUtility.singleLineHeight) * 0.5f;
            //extraSpaceBeforeIconAndLabel = KToggleWidth;
            Reload();
        }


        public override void OnGUI(Rect _rect)
        {
            base.OnGUI(_rect);
        }

        protected override void RowGUI(RowGUIArgs _args)
        {
            var tmp_Item = (TreeViewItem<ProjectListElementModel>) _args.item;
            for (int tmp_Index = 0; tmp_Index < _args.GetNumVisibleColumns(); ++tmp_Index)
            {
                CellGUI(_args.GetCellRect(tmp_Index), tmp_Item, (ColumnsType) _args.GetColumn(tmp_Index), ref _args);
            }
        }


        void CellGUI(Rect _cellRect, TreeViewItem<ProjectListElementModel> _item, ColumnsType _column,
            ref RowGUIArgs _args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref _cellRect);
            switch (_column)
            {
                case ColumnsType.Name:


                    if (_item.data.IsOpened)
                    {
                        Rect tmp_IconRect = new Rect(_args.rowRect.width - 50, _args.rowRect.y, 60,
                            _args.rowRect.height);
                        GUI.DrawTexture(tmp_IconRect, EditorGUIUtility.FindTexture("CollabEdit Icon"),
                            ScaleMode.ScaleToFit);
                    }

                    _args.rowRect = _cellRect;
                    base.RowGUI(_args);
                    break;
            }
        }

        protected override void ContextClickedItem(int _id)
        {
            base.ContextClickedItem(_id);
            clickedOnItem = true;
            GenericMenu tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("Open"), false, () =>
            {
                OnProjectOpen?.Invoke(_id);
                Reload();
            });
            tmp_Menu.AddItem(new GUIContent("Rename"), false, () => { BeginRename(FindItem(_id, rootItem)); });
            tmp_Menu.AddItem(new GUIContent("Close"), false, () =>
            {
                if (GetTreeModel.Find(_id).IsOpened)
                    OnProjectClosed?.Invoke(_id);
            });
            tmp_Menu.AddSeparator("");
            tmp_Menu.AddItem(new GUIContent("Remove"), false, () =>
            {
                if (EditorUtility.DisplayDialog("Remove Data", "Do you wanna remove these projects?", "Remove",
                    "Cancel"))
                {
                    OnProjectRemoved?.Invoke(_id);
                }
            });
            tmp_Menu.ShowAsContext();
        }


        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float _treeViewWidth)
        {
            var tmp_Columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Project Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = _treeViewWidth,
                    maxWidth = _treeViewWidth,
                    minWidth = _treeViewWidth,
                    allowToggleVisibility = false,
                    autoResize = false,
                }
            };
            Assert.AreEqual(tmp_Columns.Length, Enum.GetValues(typeof(ColumnsType)).Length,
                "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var tmp_State = new MultiColumnHeaderState(tmp_Columns);
            return tmp_State;
        }

        protected override void ContextClicked()
        {
            if (clickedOnItem)
            {
                clickedOnItem = !clickedOnItem;
                return;
            }

            base.ContextClicked();
            GenericMenu tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("New Project"), false, () =>
            {
                string tmp_ProjectPath =
                    EditorUtility.SaveFolderPanel("Set your project path", "Assets", "ARExperienceProject");
                if (string.IsNullOrEmpty(tmp_ProjectPath)) return;
                AssetDatabase.Refresh();

                string tmp_ProjectName = new DirectoryInfo(tmp_ProjectPath).Name;

                //Create folder to hold the all scripts
                string tmp_AutoScriptFolderPath = Path.Combine(tmp_ProjectPath, "Scripts");
                Directory.CreateDirectory(tmp_AutoScriptFolderPath);

                string tmp_AutoArtworkFolderPath = Path.Combine(tmp_ProjectPath, "Artwork");
                Directory.CreateDirectory(tmp_AutoArtworkFolderPath);

                string tmp_AutoEditorFolderPath = Path.Combine(Path.Combine(tmp_ProjectPath, "Scripts"), "Editor");
                Directory.CreateDirectory(tmp_AutoEditorFolderPath);

                string tmp_AutoRuntimeFolderPath = Path.Combine(Path.Combine(tmp_ProjectPath, "Scripts"), "Runtime");
                Directory.CreateDirectory(tmp_AutoRuntimeFolderPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //Create runtime assembly defined
                GenerationASMDEF(tmp_ProjectName, "Runtime", tmp_AutoRuntimeFolderPath, new List<string>(),
                    new List<string>
                    {
                        ConstKey.ARMOD_API,
                        ConstKey.PACKAGE_TOOLS,
                        ConstKey.ACTION_NOTIFICATION,
                    });


                //Create editor assembly defined
                GenerationASMDEF(tmp_ProjectName, "Editor", tmp_AutoEditorFolderPath, new List<string>
                {
                    "Editor"
                }, new List<string>
                {
                    $"{tmp_ProjectName}.Runtime"
                });

                GenerationMainEntryScript(tmp_AutoRuntimeFolderPath, tmp_ProjectName, $"{tmp_ProjectName}MainEntry");

                int tmp_Id = GetTreeModel.numberOfDataElements - 1;
                var tmp_NewProjectElement = new ProjectListElementModel(tmp_ProjectName, 0, tmp_Id, tmp_ProjectPath);
                GetTreeModel.AddElement(tmp_NewProjectElement, GetTreeModel.GetRoot, tmp_Id);

                var tmp_ConfigCache = JsonUtility.ToJson(GetTreeModel.Find(tmp_Id));
                File.WriteAllText(Path.Combine(tmp_AutoEditorFolderPath, "ConfigCacheJson.json"), tmp_ConfigCache,
                    Encoding.UTF8);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            });
            tmp_Menu.AddSeparator("");
            tmp_Menu.AddItem(new GUIContent("Open Project"), false, () =>
            {
                string tmp_ProjectPath =
                    EditorUtility.OpenFolderPanel("Set your project path", "Assets", "ARExperienceProject");
                if (string.IsNullOrEmpty(tmp_ProjectPath)) return;
                AssetDatabase.Refresh();
                if (!Utility.IsCorrectProjectStructure(tmp_ProjectPath))
                {
                    Debug.LogError("Not Correct project structure!");
                    return;
                }


                string tmp_ProjectName = new DirectoryInfo(tmp_ProjectPath).Name;

                //GetTreeModel.numberOfDataElements length is 1 always -- Root Node  
                var tmp_ProjectCount = GetTreeModel.numberOfDataElements;
                for (int tmp_Index = 0; tmp_Index < tmp_ProjectCount - 1; tmp_Index++)
                {
                    var tmp_QueryElement = GetTreeModel.Find(tmp_Index);
                    if (!tmp_QueryElement.Name.Equals(tmp_ProjectName)) continue;
                    Debug.LogError($"Error! [{tmp_ProjectName}]-Project is exist!");
                    return;
                }

                int tmp_Id = GetTreeModel.numberOfDataElements - 1;

                Utility.ReadProjectConfigCache(Path.Combine(tmp_ProjectPath, "AutomaticGenerated/ConfigCacheJson.json"),
                    out var tmp_ProjectListElementModel);
                tmp_ProjectListElementModel ??=
                    new ProjectListElementModel(tmp_ProjectName, 0, tmp_Id, tmp_ProjectPath);
                tmp_ProjectListElementModel.Id = tmp_Id;

                //Check whether the project is consistent with the previous location.
                //Inconsistency will cause errors, so we update the path in the cache to the current project path.
                //ERROR:https://www.teambition.com/task/60c4b986ab475d00440770de
                var tmp_CurrentShortProject = Utility.ShortenPath(tmp_ProjectPath);
                if (String.CompareOrdinal(tmp_ProjectListElementModel.SubPath, tmp_CurrentShortProject) !=
                    0)
                {
                    tmp_ProjectListElementModel.SubPath = tmp_CurrentShortProject;
                    Debug.LogWarning("Fix project path");
                }


                GetTreeModel.AddElement(tmp_ProjectListElementModel, GetTreeModel.GetRoot, tmp_Id);

                OnProjectOpen?.Invoke(tmp_Id);
            });
            tmp_Menu.ShowAsContext();
        }


        private void GenerationMainEntryScript(string _destFilePath, string _namespace,
            string _scriptName)
        {
            var tmp_BaseDir = $"Packages/{ConstKey.PACKAGE_NAME}/Editor/";

            var tmp_EntryScriptPath = Path.GetFullPath($"{tmp_BaseDir}/EntryTemplate.txt");
            var tmp_EntryScriptText = File.ReadAllText(tmp_EntryScriptPath);
            tmp_EntryScriptText = tmp_EntryScriptText.Replace("#NAMESPACE#", _namespace);
            tmp_EntryScriptText = tmp_EntryScriptText.Replace("#SCRIPTNAME#", _scriptName);
            File.WriteAllText(Path.Combine(_destFilePath, _scriptName + ".cs"), tmp_EntryScriptText, Encoding.UTF8);
        }


        private void GenerationASMDEF(string _fileName, string _suffix, string _path, List<string> _includePlatforms,
            List<string> _references)
        {
            var tmp_FileName = !string.IsNullOrEmpty(_suffix) ? $"{_fileName}.{_suffix}" : _fileName;

            //Create assembly defined
            var tmp_RuntimeAssemblyDefined = new AssemblyDataModel
            {
                name = tmp_FileName,
                autoReferenced = true,
                includePlatforms = _includePlatforms,
                references = _references
            };

            string tmp_RuntimeAssemblyDefinedJson = JsonUtility.ToJson(tmp_RuntimeAssemblyDefined);
            File.WriteAllText(Path.Combine(_path, $"{tmp_FileName}.asmdef"),
                tmp_RuntimeAssemblyDefinedJson);
        }
    }
}