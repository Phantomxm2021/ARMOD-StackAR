using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class DrawBuildView : ISubView
    {
        private bool displayHelper;
        private readonly ProjectDataModel projectDataModel;
        private bool currentSceneWasChanged;

        public DrawBuildView(ProjectDataModel _projectDataModel)
        {
            projectDataModel = _projectDataModel;
        }



        public void DrawSubView(Rect _area)
        {
            if (projectDataModel == null) return;
            DrawBuild();
        }

        public void Dispose()
        {
        }


        private string GetScriptAssembliesFolder => Application.dataPath.Replace("Assets", "Library/ScriptAssemblies/");

        private void DrawBuild()
        {
            var tmp_ProjectData = projectDataModel.GetOpeningProject();
            // ReSharper disable once InconsistentNaming
            var tmp_ARProperty = projectDataModel.GetOpeningProjectConfigure();
            var tmp_BuildSettingData = projectDataModel.GetOpeningProject().BuildSettingData;
            var tmp_AssetData = projectDataModel.GetOpeningProject().GetContentTreeElements;

            tmp_BuildSettingData.BuildTarget =
                (BuildTarget) EditorGUILayout.EnumPopup("Build Platform", tmp_BuildSettingData.BuildTarget);

            tmp_BuildSettingData.BuildTargetGroup =
                (BuildTargetGroup) EditorGUILayout.EnumPopup("Platform Group", tmp_BuildSettingData.BuildTargetGroup);


            tmp_BuildSettingData.BuildCompression =
                (BuildCompressionType) EditorGUILayout.EnumPopup("Compression", tmp_BuildSettingData.BuildCompression);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build your own ARExperience"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    ExecuteBuildAssetBundle(tmp_ProjectData, tmp_ARProperty, tmp_AssetData, tmp_BuildSettingData);
                }
            }

            if (GUILayout.Button("Show in file browser"))
            {
                var tmp_Path = Path.Combine(Application.dataPath.Replace("Assets", ""),
                    $"ServerData/{tmp_BuildSettingData.BuildTarget.ToString().ToLower()}" +
                    $"/{tmp_ProjectData.Name.ToLower()}/{tmp_ProjectData.Name.ToLower()}.arexperience");
                EditorUtility.RevealInFinder(tmp_Path);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(50);
            EditorGUILayout.LabelField("FAQ");

            displayHelper =
                EditorGUILayout.BeginFoldoutHeaderGroup(displayHelper, "Where is the AR experience bundle?");
            if (displayHelper)
            {
                EditorGUILayout.HelpBox(
                    "After the packaging is successful, your AR experience will be stored in" +
                    "\nthe root directory of your project [ServerData/Platform/Project Name]",
                    MessageType.Info);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void ExecuteBuildAssetBundle(ProjectListElementModel tmp_ProjectData, Configures tmp_ARProperty,
            List<ContentListElementModel> tmp_AssetData, BuildSettingData tmp_BuildSettingData)
        {
            List<string> tmp_AddressableName = new List<string>();
            List<string> tmp_BundlePath = new List<string>();


            string tmp_ProjectAutoGenerationPath = Path.Combine(tmp_ProjectData.GetProjectPath(), "AutomaticGenerated");
            if (!Directory.Exists(tmp_ProjectAutoGenerationPath))
                Directory.CreateDirectory(tmp_ProjectAutoGenerationPath);
            // ReSharper disable once InconsistentNaming
            var tmp_ARPropertyJsonFilePath = Path.Combine(tmp_ProjectAutoGenerationPath, "ARProperty.json");
            File.WriteAllText(tmp_ARPropertyJsonFilePath, JsonUtility.ToJson(tmp_ARProperty));

            if (!string.IsNullOrEmpty(tmp_ARProperty.DomainName))
            {
                var tmp_SourceFilePath = Path.Combine(GetScriptAssembliesFolder,
                    new DirectoryInfo(tmp_ProjectData.GetProjectPath()).Name);

                string tmp_DllAssetPath =
                    Utility.CopyFileToProject($"{tmp_SourceFilePath}.runtime.dll", tmp_ProjectAutoGenerationPath,
                        "bytes");
                string tmp_PdbAssetPath = Utility.CopyFileToProject($"{tmp_SourceFilePath}.runtime.pdb",
                    tmp_ProjectAutoGenerationPath, "bytes");

                //Not allowed to be empty
                if (string.IsNullOrEmpty(tmp_DllAssetPath) || string.IsNullOrEmpty(tmp_PdbAssetPath))
                    return;

                var tmp_RenameDllFileToLower = Path.GetFileName(tmp_DllAssetPath).Replace(".bytes", "").ToLower();
                var tmp_RenamePdbFileToLower = Path.GetFileName(tmp_PdbAssetPath).Replace(".bytes", "").ToLower();
                tmp_AddressableName.Add(tmp_RenameDllFileToLower);
                tmp_AddressableName.Add(tmp_RenamePdbFileToLower);
                tmp_BundlePath.Add(tmp_DllAssetPath);
                tmp_BundlePath.Add(tmp_PdbAssetPath);
            }

            tmp_AddressableName.Add(Path.GetFileNameWithoutExtension(tmp_ARPropertyJsonFilePath));
            tmp_BundlePath.Add(Utility.ShortenPath(tmp_ARPropertyJsonFilePath));


            foreach (var tmp_ContentData in tmp_AssetData)
            {
                if (string.IsNullOrEmpty(tmp_ContentData.AssetPath)) continue;
                if (string.IsNullOrEmpty(tmp_ContentData.Name)) continue;

                tmp_AddressableName.Add(Path.GetFileName(tmp_ContentData.Name));
                tmp_BundlePath.Add(tmp_ContentData.AssetPath);
            }

            var tmp_SavedBundlePath = Path.Combine(Application.dataPath.Replace("Assets", "ServerData"),
                tmp_BuildSettingData.BuildTarget.ToString().ToLower());
            tmp_SavedBundlePath = Path.Combine(tmp_SavedBundlePath, tmp_ProjectData.Name.ToLower());

            if (!Directory.Exists(tmp_SavedBundlePath))
                Directory.CreateDirectory(tmp_SavedBundlePath);

            BuildCompression tmp_BuildCompression = BuildCompression.LZ4;
            switch (tmp_ProjectData.BuildSettingData.BuildCompression)
            {
                case BuildCompressionType.Uncompressed:
                    tmp_BuildCompression = BuildCompression.Uncompressed;
                    break;
                case BuildCompressionType.LZ4:
                    tmp_BuildCompression = BuildCompression.LZ4;
                    break;
                case BuildCompressionType.LZMA:
                    tmp_BuildCompression = BuildCompression.LZMA;
                    break;
                case BuildCompressionType.UncompressedRuntime:
                    tmp_BuildCompression = BuildCompression.UncompressedRuntime;
                    break;
                case BuildCompressionType.LZ4Runtime:
                    tmp_BuildCompression = BuildCompression.LZ4Runtime;
                    break;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var tmp_StateCode = BundlePacker.ScriptableBuildPipeline(tmp_ProjectData.Name.ToLower(),
                tmp_AddressableName,
                tmp_BundlePath,
                tmp_BuildSettingData.BuildTarget,
                tmp_BuildSettingData.BuildTargetGroup,
                tmp_BuildCompression,
                tmp_SavedBundlePath
            );

            if (tmp_StateCode == ReturnCode.UnsavedChanges)
            {
                MainView.ShowNotify("Your Scene was changed,But you not saved changes");
            }

            AssetDatabase.Refresh();
            GUIUtility.ExitGUI();
        }


        private void ResponseToStatusCode(ReturnCode _returnCode)
        {
            switch (_returnCode)
            {
                case ReturnCode.UnsavedChanges:
                    if (!EditorSceneManager.SaveOpenScenes())
                    {
                        Debug.LogError("You need to save the current scene before you can build AssetBundle!");
                    }

                    break;
            }
        }
    }
}