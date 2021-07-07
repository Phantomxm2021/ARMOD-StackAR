using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class FaceMeshBlock:AbstractBlock
    {
        
        private bool faceMeshBlockToggle = true;
        private readonly Configures property;
        public FaceMeshBlock(){}

        public FaceMeshBlock(Configures _property) : base(_property)
        {
            property = _property;
        }
        
        public override Rect DrawBlock(Rect _area)
        {
            faceMeshBlockToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(faceMeshBlockToggle, "Face Mesh", null,
                    ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();
            if (faceMeshBlockToggle)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.MaximumFaceCount =
                    EditorGUILayout.IntField("Maximum Face Count", property.MaximumFaceCount);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
                Utility.DrawHorizontalDivLine();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return tmp_BlockRect;
        }

        void ShowHeaderContextMenu(Rect _position)
        {
            var tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("Reset"), false, () => OnRemoved());
            tmp_Menu.AddItem(new GUIContent(CONST_HELP_BUTTON_TITLE), false, () => OpenReference());
            tmp_Menu.DropDown(_position);
        }


        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}features#face-mesh");
            return false;
        }
        
        public override  bool OnRemoved()
        {
            property.MaximumFaceCount = 1;
            return true;
        }
    }
}