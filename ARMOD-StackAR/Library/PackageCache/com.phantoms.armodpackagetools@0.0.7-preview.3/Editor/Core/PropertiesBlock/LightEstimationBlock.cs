using com.Phantoms.ARMODPackageTools.Core;
using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class LightEstimationBlock : AbstractBlock
    {
        private bool lightEstimationBlockToggle = true;
        private readonly Configures property;
        public LightEstimationBlock(){}

        public LightEstimationBlock(Configures _property) : base(_property)
        {
            property = _property;
        }

        public override Rect DrawBlock(Rect _area)
        {
            lightEstimationBlockToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(lightEstimationBlockToggle, "Light Estimation",null,ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();
            if (lightEstimationBlockToggle)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                
                property.LightEstimation =
                    EditorGUILayout.Toggle("Light Estimation", property.LightEstimation);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
                
                Utility.DrawHorizontalDivLine();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return tmp_BlockRect;
        }

        public override  bool OnRemoved()
        {
            property.LightEstimation = false;
            return true;
        }
        
        
        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}features#light-estimation");
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