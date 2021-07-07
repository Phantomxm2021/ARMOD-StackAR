using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ProgrammableBlock : AbstractBlock
    {
        private readonly Configures property;
        private bool foldoutToggle = true;
        private const string CONST_SCRIPTABLE_BLOCK_NAME = "Programmable";


        public ProgrammableBlock(Configures _property) : base(_property)
        {
            property = _property;

            if (!string.IsNullOrEmpty(property.ProjectName))
            {
                property.DomainName = property.ProjectName;
                property.MainEntry = $"{property.ProjectName}MainEntry";
            }
        }

        public override Rect DrawBlock(Rect _area)
        {
            foldoutToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(foldoutToggle, CONST_SCRIPTABLE_BLOCK_NAME, null,
                    ShowHeaderContextMenu);
            var tmp_HeaderRect = GUILayoutUtility.GetLastRect();
            if (foldoutToggle)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();

                property.DomainName =
                    EditorGUILayout.TextField(new GUIContent("Domain Name"), property.DomainName);
                property.MainEntry = EditorGUILayout.TextField(new GUIContent("Main Entry"), property.MainEntry);
                property.DebugModel = EditorGUILayout.Toggle(new GUIContent("Debug-Model"), property.DebugModel);
               
                if (property.DebugModel)
                {
                    EditorGUILayout.HelpBox("Checking Debug-Model will affect the actual performance!",
                        MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                Utility.DrawHorizontalDivLine();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return tmp_HeaderRect;
        }

        public override bool OnRemoved()
        {
            property.MainEntry = property.DomainName = null;
            return true;
        }

        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}programmable");
            return false;
        }

        void ShowHeaderContextMenu(Rect _position)
        {
            var tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("Reset"), false, () => OnRemoved());
            tmp_Menu.AddItem(new GUIContent(CONST_HELP_BUTTON_TITLE), false, () => OpenReference());
            tmp_Menu.DropDown(_position);
        }
    }
}