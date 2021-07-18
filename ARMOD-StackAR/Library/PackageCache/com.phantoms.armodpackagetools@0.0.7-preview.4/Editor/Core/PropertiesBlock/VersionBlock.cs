using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class VersionBlock : AbstractBlock
    {
        private readonly Configures property;
        private bool enabledArAlgorithm = true;
        private const string enableARAlgorithmMsg = "Version Checker";

        public VersionBlock(Configures _configures)
        {
            property = _configures;
        }

        public override Rect DrawBlock(Rect _area)
        {
            enabledArAlgorithm = EditorGUILayout.BeginFoldoutHeaderGroup(enabledArAlgorithm, enableARAlgorithmMsg, null,
                ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();

            if (enabledArAlgorithm)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.Version = EditorGUILayout.TextField(new GUIContent("SDK Version"),property.Version);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
                Utility.DrawHorizontalDivLine();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return tmp_BlockRect;
        }

        public override bool OnRemoved()
        {
            property.Version = "0.0.1";
            return false;
        }
        
        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}version-checker-block");
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