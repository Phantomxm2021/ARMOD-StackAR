using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class AlgorithmBlock : AbstractBlock
    {
        private readonly Configures property;
        private bool enabledArAlgorithm = true;
        private string enableARAlgorithmMsg = "AR Algorithm";


        public AlgorithmBlock(Configures _property) : base(_property)
        {
            property = _property;
        }

        public override Rect DrawBlock(Rect _area)
        {
            enabledArAlgorithm = EditorGUILayout.BeginFoldoutHeaderGroup(enabledArAlgorithm, enableARAlgorithmMsg, null,
                ShowHeaderContextMenu);
            var tmp_HeaderRect = GUILayoutUtility.GetLastRect();

            if (enabledArAlgorithm)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical();
                property.Algorithm =
                    (AlgorithmType) EditorGUILayout.EnumPopup("AR Algorithm ", property.Algorithm);

                switch (property.Algorithm)
                {
                    case AlgorithmType.ImageTracker:
                        property.MaxMovingOfTracking =
                            EditorGUILayout.IntField(
                                new GUIContent("Max Moving Of Tracking",
                                    "You can specify the number of moving images to track simultaneously. "),
                                property.MaxMovingOfTracking);
                        break;
                    case AlgorithmType.FocusSlam:
                        property.PlaneDetectionMode =
                            (PlaneDetectionMode) EditorGUILayout.EnumFlagsField("Plane Detection Mode",
                                property.PlaneDetectionMode);
                        break;
                    case AlgorithmType.Immersal:
                        EditorGUILayout.Separator();
                        property.DeveloperToken = EditorGUILayout.TextField("Developer Token", property.DeveloperToken);
                        break;
                }

                property.AlgorithmAutoStart =
                    EditorGUILayout.Toggle(new GUIContent("Algorithm Auto Start",
                            "Toggling this on/off will enable/disable the automatic startup of algorithm at runtime"),
                        property.AlgorithmAutoStart);
                if (!property.AlgorithmAutoStart)
                    EditorGUILayout.HelpBox(
                        "If `algorithm auto start` is disabled, you need to start it manually by script.",
                        MessageType.Warning);

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
            property.PlaneDetectionMode = PlaneDetectionMode.None;
            property.Algorithm = AlgorithmType.FocusSlam;
            return false;
        }

        public override bool OpenReference()
        {
            Application.OpenURL($"{CONST_HELP_BASE_URL}ar-algorithm-block#ar-algorithm");
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