using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class PostProcessingBlock : AbstractBlock
    {
        private bool postProcessingToggle = true;
        private readonly Configures property;

        public PostProcessingBlock()
        {
        }

        public PostProcessingBlock(Configures _property) : base(_property)
        {
            property = _property;
        }

        public override Rect DrawBlock(Rect _area)
        {
            postProcessingToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(postProcessingToggle, "Post processing Feature", null,
                    ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();
            if (postProcessingToggle)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.PostProcessing =
                    EditorGUILayout.Toggle("Post processing", property.PostProcessing);
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
            property.PostProcessing = false;
            return true;
        }

        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}features#post-processing-feature");
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