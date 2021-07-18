using System.Collections.Generic;
using Action_Notification_Manager.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using com.Phantoms.ActionNotification.Runtime;

namespace com.Phantoms.ActionNotification.Editor
{
    public class ANMEditor : EditorWindow
    {
        private ActionParamaterData actionParamaterData;
        private bool foldout;
        private Vector3 scrollView;

        //[MenuItem("Tools/AR-MOD/ANM/Keys Generator Window")]
        static void ANMWindow()
        {
            var tmp_Window = GetWindow<ANMEditor>();
            tmp_Window.Show();
        }

        private void OnGUI()
        {
            if (actionParamaterData == null)
                actionParamaterData = Resources.Load<ActionParamaterData>("ANMKeys");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add New Keys Field"))
            {
                actionParamaterData.ObserverKeys.Add(string.Empty);
            }

            if (GUILayout.Button("Generate New Keys"))
            {
                GenerateParamater();
            }

            EditorGUILayout.EndHorizontal();

            foldout = EditorGUILayout.Foldout(foldout, "Keys");
            if (foldout)
            {
                scrollView = EditorGUILayout.BeginScrollView(scrollView);
                for (int tmp_Index = 0; tmp_Index < actionParamaterData.ObserverKeys.Count; tmp_Index++)
                {
                    EditorGUILayout.BeginHorizontal();
                    actionParamaterData.ObserverKeys[tmp_Index] =
                        EditorGUILayout.TextField(actionParamaterData.ObserverKeys[tmp_Index]);
                    if (GUILayout.Button("Remove"))
                    {
                        actionParamaterData.ObserverKeys.RemoveAt(tmp_Index);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        public void GenerateParamater()
        {
            //TODO:4.create cs file

            var tmp_FilePath =
                EditorUtility.SaveFilePanelInProject("Create Data Enum Script",
                    "ActionParamaterDataEnum",
                    "cs",
                    "");

            if (string.IsNullOrEmpty(tmp_FilePath))
            {
                Debug.LogError("Failed to create file! You need to choose a file path to create.");
                return;
            }

//            var tmp_GenerateFilePath = Path.Combine("Packages/Action Notification Manager/Runtime",
//                "ActionParamaterDataEnum.cs");

            using (StreamWriter tmp_StreamWriter = new StreamWriter(tmp_FilePath))
            {
                tmp_StreamWriter.WriteLine("namespace ActionNotification.Runtime");
                tmp_StreamWriter.WriteLine("{");
                tmp_StreamWriter.WriteLine("    public enum ActionParameterDataType");
                tmp_StreamWriter.WriteLine("    {");
                foreach (var tmp_T in actionParamaterData.ObserverKeys)
                {
                    tmp_StreamWriter.WriteLine("    \t" + tmp_T + ",");
                }

                tmp_StreamWriter.WriteLine("    }");
                tmp_StreamWriter.WriteLine("}");
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif