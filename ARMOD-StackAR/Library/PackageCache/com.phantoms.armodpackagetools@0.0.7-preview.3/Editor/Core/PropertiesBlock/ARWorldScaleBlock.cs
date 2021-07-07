using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ARWorldScaleBlock:AbstractBlock
    {
        private readonly Configures property;
        private bool enabledArAlgorithm = true;
        private const string enableARAlgorithmMsg = "AR World Scale";

        
        public ARWorldScaleBlock(Configures _configures)
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
                property.ARWorldScale = EditorGUILayout.FloatField(new GUIContent("AR World Scale"),property.ARWorldScale);
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
            property.ARWorldScale = 1f;
            return false;
        }
        
        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}/features#arworld-scale-block");
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