using UnityEditor;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class PointCloudVisualizerBlock : AbstractBlock
    {
        private readonly Configures property;
        private bool pointCloudVisualizerToggle = true;
        private string enabledCustomPointCloudVisualizer = "Point Cloud Visualizer";
        private const string CONST_DEFAULT_POINT_CLOUD_VISUALIZER = "ARDefaultPointCloudVisualizer";

        public PointCloudVisualizerBlock(Configures _property) : base(_property)
        {
            property = _property;
        }

        public override Rect DrawBlock(Rect _area)
        {
            pointCloudVisualizerToggle =
                EditorGUILayout.BeginFoldoutHeaderGroup(pointCloudVisualizerToggle,
                    enabledCustomPointCloudVisualizer, null, ShowHeaderContextMenu);
            var tmp_BlockRect = GUILayoutUtility.GetLastRect();
            if (pointCloudVisualizerToggle)
            {
               
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.CustomPointCloudVisualizerName =
                    EditorGUILayout.TextField(new GUIContent("Point Cloud Visualizer Name"),
                        property.CustomPointCloudVisualizerName);
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
            property.CustomPointCloudVisualizerName = null;
            return true;
        }

        public bool SetToDefault()
        {
            property.CustomPointCloudVisualizerName = CONST_DEFAULT_POINT_CLOUD_VISUALIZER;
            return true;
        }

        
        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}visualizer#point-cloud-visualizer");
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