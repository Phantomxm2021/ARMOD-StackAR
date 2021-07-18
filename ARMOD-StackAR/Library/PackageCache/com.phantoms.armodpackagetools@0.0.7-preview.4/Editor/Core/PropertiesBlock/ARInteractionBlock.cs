using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ARInteractionBlock : AbstractBlock
    {
        private Configures configures;
        private bool canInteractionToggle;

        public ARInteractionBlock()
        {
        }

        public ARInteractionBlock(Configures _configures) : base(_configures)
        {
            configures = _configures;
        }

        public override Rect DrawBlock(Rect _area)
        {
            canInteractionToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(canInteractionToggle, "AR Interaction Feature", null,
                    ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();
            if (canInteractionToggle)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                configures.CanInteraction =
                    EditorGUILayout.Toggle("AR Interaction", configures.CanInteraction);

                configures.AutoSelect =
                    EditorGUILayout.Toggle("Auto Select", configures.AutoSelect);

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
            configures.CanInteraction = false;
            configures.AutoSelect = false;
            return true;
        }
        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}features#ar-interaction-feature");
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