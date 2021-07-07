using System.Collections.Generic;
using System.IO;
using com.Phantoms.ARMODPackageTools.Core;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace com.Phantoms.ARMODSimulator.Editor
{
    [InitializeOnLoad]
    public class BuildAndRunSimulator
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle CommandButtonStyle;

            static ToolbarStyles()
            {
                CommandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        private static Texture2D ICON_TEXTURE_2D;
        private static ProjectDataModel PROJECT_DATA_MODEL;

        private static string GetScriptAssembliesFolder =>
            Application.dataPath.Replace("Assets", "Library/ScriptAssemblies/");

        static BuildAndRunSimulator()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            ICON_TEXTURE_2D =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Packages/com.phantoms.armodsimulator/Assets/Textures/launch.png");
            PROJECT_DATA_MODEL =
                AssetDatabase.LoadAssetAtPath<ProjectDataModel>(Utility.GetRootDataPath("PROJECT_DATA_MODEL.asset"));
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (!GUILayout.Button(new GUIContent(ICON_TEXTURE_2D, "Play after building AR experience"),
                ToolbarStyles.CommandButtonStyle)) return;
            if (!ExecuteBuildAssetBundle()) return;
            EditorApplication.isPlaying = true;
        }

        private static bool ExecuteBuildAssetBundle()
        {
            List<string> tmp_AddressableName = new List<string>();
            List<string> tmp_BundlePath = new List<string>();


            string tmp_ProjectAutoGenerationPath =
                Path.Combine(GetProjectData().GetProjectPath(), "AutomaticGenerated");
            if (!Directory.Exists(tmp_ProjectAutoGenerationPath))
                Directory.CreateDirectory(tmp_ProjectAutoGenerationPath);
            // ReSharper disable once InconsistentNaming
            var tmp_ARPropertyJsonFilePath = Path.Combine(tmp_ProjectAutoGenerationPath, "ARProperty.json");
            File.WriteAllText(tmp_ARPropertyJsonFilePath, JsonUtility.ToJson(GetARProperty()));

            if (!string.IsNullOrEmpty(GetARProperty().DomainName))
            {
                var tmp_SourceFilePath = Path.Combine(GetScriptAssembliesFolder,
                    new DirectoryInfo(GetProjectData().GetProjectPath()).Name);

                string tmp_DllAssetPath =
                    Utility.CopyFileToProject($"{tmp_SourceFilePath}.runtime.dll", tmp_ProjectAutoGenerationPath,
                        "bytes");
                string tmp_PdbAssetPath = Utility.CopyFileToProject($"{tmp_SourceFilePath}.runtime.pdb",
                    tmp_ProjectAutoGenerationPath, "bytes");

                //Not allowed to be empty
                if (string.IsNullOrEmpty(tmp_DllAssetPath) || string.IsNullOrEmpty(tmp_PdbAssetPath))
                {
                    Debug.LogError("Can not found dll assets!");
                    return false;
                }

                var tmp_RenameDllFileToLower = Path.GetFileName(tmp_DllAssetPath).Replace(".bytes", "").ToLower();
                var tmp_RenamePdbFileToLower = Path.GetFileName(tmp_PdbAssetPath).Replace(".bytes", "").ToLower();
                tmp_AddressableName.Add(tmp_RenameDllFileToLower);
                tmp_AddressableName.Add(tmp_RenamePdbFileToLower);
                tmp_BundlePath.Add(tmp_DllAssetPath);
                tmp_BundlePath.Add(tmp_PdbAssetPath);
            }

            tmp_AddressableName.Add(Path.GetFileNameWithoutExtension(tmp_ARPropertyJsonFilePath));
            tmp_BundlePath.Add(Utility.ShortenPath(tmp_ARPropertyJsonFilePath));


            foreach (var tmp_ContentData in GetAssetsData())
            {
                if (string.IsNullOrEmpty(tmp_ContentData.AssetPath)) continue;
                if (string.IsNullOrEmpty(tmp_ContentData.Name)) continue;

                tmp_AddressableName.Add(Path.GetFileName(tmp_ContentData.Name));
                tmp_BundlePath.Add(tmp_ContentData.AssetPath);
            }

            var tmp_SavedBundlePath = Path.Combine(Application.dataPath.Replace("Assets", "ServerData"),
                GetBuildSettingData().BuildTarget.ToString().ToLower());
            tmp_SavedBundlePath = Path.Combine(tmp_SavedBundlePath, GetProjectData().Name.ToLower());

            if (!Directory.Exists(tmp_SavedBundlePath))
                Directory.CreateDirectory(tmp_SavedBundlePath);

            BuildCompression tmp_BuildCompression = BuildCompression.LZ4;
            switch (GetProjectData().BuildSettingData.BuildCompression)
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

            var tmp_StateCode = BundlePacker.ScriptableBuildPipeline(GetProjectData().Name.ToLower(),
                tmp_AddressableName,
                tmp_BundlePath,
                GetBuildSettingData().BuildTarget,
                GetBuildSettingData().BuildTargetGroup,
                tmp_BuildCompression,
                tmp_SavedBundlePath
            );

            return tmp_StateCode == ReturnCode.Success;
        }


        private static List<ContentListElementModel> GetAssetsData()
        {
            return PROJECT_DATA_MODEL
                .ProjectTreeElements[PROJECT_DATA_MODEL.CurrentContentViewId].GetContentTreeElements;
        }

        private static BuildSettingData GetBuildSettingData()
        {
            return PROJECT_DATA_MODEL
                .ProjectTreeElements[PROJECT_DATA_MODEL.CurrentContentViewId].BuildSettingData;
        }

        private static Configures GetARProperty()
        {
            return PROJECT_DATA_MODEL
                .ProjectTreeElements[PROJECT_DATA_MODEL.CurrentContentViewId].Configures;
        }

        private static ProjectListElementModel GetProjectData()
        {
            return PROJECT_DATA_MODEL
                .ProjectTreeElements[PROJECT_DATA_MODEL.CurrentContentViewId];
        }
    }
}