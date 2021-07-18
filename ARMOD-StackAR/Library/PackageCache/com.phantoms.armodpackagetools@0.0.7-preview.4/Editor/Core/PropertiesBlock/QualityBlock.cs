using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class QualityBlock : AbstractBlock
    {
        private Configures property;
        private bool enabledQualityBlock = true;
        private const string ENABLE_QUALITY_BLOCK_MSG = "Quality";

        private enum QualityLevel : byte
        {
            Low,
            Medium,
            High
        }

        private QualityLevel qualityLevel;

        public QualityBlock(Configures _configures)
        {
            property = _configures;
            qualityLevel =(QualityLevel)_configures.QualityLevel;
        }

        public override Rect DrawBlock(Rect _area)
        {
            enabledQualityBlock = EditorGUILayout.BeginFoldoutHeaderGroup(enabledQualityBlock, ENABLE_QUALITY_BLOCK_MSG,
                null, ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();

            if (enabledQualityBlock)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                qualityLevel = (QualityLevel) EditorGUILayout.EnumPopup("Quality Level", qualityLevel);
                property.QualityLevel = (byte) qualityLevel;
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
            property.QualityLevel = 1;
            return true;
        }

        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}graphics");
            return true;
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