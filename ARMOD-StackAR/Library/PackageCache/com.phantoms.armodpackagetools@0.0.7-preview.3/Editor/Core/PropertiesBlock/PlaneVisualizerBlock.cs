using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class PlaneVisualizerBlock : AbstractBlock
    {
        private readonly Configures property;
        private bool planeVisualizerToggle = true;
        private string enabledCustomPlaneVisualizer = "Custom Plane Visualizer";
        private const string CONST_DEFAULT_PLANE_VISUALIZER = "ARDefaultPlaneVisualizer";

        public PlaneVisualizerBlock(Configures _property) : base(_property)
        {
            property = _property;
        }

        public override Rect DrawBlock(Rect _area)
        {
            planeVisualizerToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(planeVisualizerToggle, enabledCustomPlaneVisualizer, null,
                    ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();
            if (planeVisualizerToggle)
            {
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.CustomPlaneVisualizerName =
                    EditorGUILayout.TextField(new GUIContent("Visualizer Name"),
                        property.CustomPlaneVisualizerName);
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
            property.CustomPlaneVisualizerName = null;
            return true;
        }

        private bool SetToDefault()
        {
            property.CustomPlaneVisualizerName = CONST_DEFAULT_PLANE_VISUALIZER;
            return true;
        }
        
        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}visualizer#custom-plane-visualizer");
            return false;
        }
        
        void ShowHeaderContextMenu(Rect _position)
        {
            var tmp_Menu = new GenericMenu();
            tmp_Menu.AddItem(new GUIContent("Default"), false, () => SetToDefault());
            tmp_Menu.AddItem(new GUIContent("Reset"), false, () => OnRemoved());
            tmp_Menu.AddItem(new GUIContent(CONST_HELP_BUTTON_TITLE), false, () => OpenReference());
            tmp_Menu.DropDown(_position);
        }
    }
}