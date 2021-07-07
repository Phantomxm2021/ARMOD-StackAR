using com.Phantoms.ActionNotification.Runtime;
using com.Phantoms.ARMODSimulator.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace com.Phantoms.ARMODSimulator.Editor
{
    [CustomEditor(typeof(Simulator))]
    public class SimulatorEditor : UnityEditor.Editor
    {
        private bool isdisplay;

        [MenuItem("Tools/AR-MOD/Simulator")]
        private static void CreateSimulator()
        {
            EditorSceneManager.newSceneCreated += (_scene, _setup, _mode) =>
            {
                var tmp_SimulatorPrefab = Resources.Load<GameObject>("Simulator");
                GameObject tmp_Simulator = Instantiate(tmp_SimulatorPrefab);
                tmp_Simulator.name = tmp_Simulator.name.Replace("(Clone)", "");
            };
            
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var tmp_Simulator = target as Simulator;

            if (tmp_Simulator == null) return;

            if (string.IsNullOrEmpty(tmp_Simulator.ProjectName))
            {
                EditorGUILayout.HelpBox("Your project name is empty!", MessageType.Error);
            }
            else
            {
                if (GUILayout.Button("Test Project"))
                {
                    tmp_Simulator.StartEmulating();
                }
            }


            var tmp_Config = tmp_Simulator.config;
            if (tmp_Config == null) return;

            isdisplay = EditorGUILayout.Foldout(isdisplay, "Event Config Details");
            if (!isdisplay) return;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.HelpBox("Event Data", MessageType.Info);
            EditorGUILayout.Space();

            switch (tmp_Config)
            {
                case FocusResultNotificationDataConfig tmp_FocusResult:
                    DrawFocusSetting(tmp_FocusResult);
                    break;
                case AnchorNotificationDataConfig tmp_Anchor:
                    DrawAnchorSetting(tmp_Anchor);
                    break;
                case AlgorithmSetterNotificationDataConfig tmp_AlgorithmSetter:
                    DrawAlgorithmSetting(tmp_AlgorithmSetter);
                    break;
                case MarkerNotificationDataConfig tmp_Marker:
                    DrawMarkerSetting(tmp_Marker);
                    break;
                default:
                    tmp_Config.ActionName = EditorGUILayout.TextField("Action Name", tmp_Config.ActionName);
                    break;
            }

            if (GUILayout.Button("Send Event"))
            {
                tmp_Simulator.SendEvent();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawMarkerSetting(MarkerNotificationDataConfig _marker)
        {
            _marker.ActionName = EditorGUILayout.TextField("Action Name", _marker.ActionName);
            _marker.MarkerName = EditorGUILayout.TextField("Marker Name", _marker.MarkerName);
            _marker.MarkerState = (MarkerTrackingState)EditorGUILayout.EnumPopup("Marker State", _marker.MarkerState);
            _marker.IsSupport = EditorGUILayout.Toggle("Is Support", _marker.IsSupport);
            _marker.MarkerTrackable = (Transform) EditorGUILayout.ObjectField("Marker Trackable",
                _marker.MarkerTrackable, typeof(Transform), true);
        }

        private void DrawAlgorithmSetting(AlgorithmSetterNotificationDataConfig _algorithmSetter)
        {
            _algorithmSetter.ActionName = EditorGUILayout.TextField("Action Name", _algorithmSetter.ActionName);
            _algorithmSetter.AlgorithmState =
                EditorGUILayout.Toggle("Algorithm State", _algorithmSetter.AlgorithmState);
        }

        private void DrawAnchorSetting(AnchorNotificationDataConfig _anchor)
        {
            _anchor.ActionName = EditorGUILayout.TextField("Action Name", _anchor.ActionName);
            _anchor.Offset = EditorGUILayout.Vector3Field("Offset", _anchor.Offset);
            _anchor.Position = EditorGUILayout.Vector3Field("Position", _anchor.Position);
            _anchor.Rotation.eulerAngles = EditorGUILayout.Vector4Field("Rotation", _anchor.Rotation.eulerAngles);
            _anchor.StickType =
                (AnchorNotificationData.StickTypeEnum) EditorGUILayout.EnumPopup("Stick Type", _anchor.StickType);
            _anchor.TrackableType =
                (AnchorNotificationData.TrackableTypeEnum) EditorGUILayout.EnumPopup("Trackable Type",
                    _anchor.TrackableType);
            _anchor.ControllerTargetNode = (GameObject) EditorGUILayout.ObjectField("ControllerTargetNode",
                _anchor.ControllerTargetNode, typeof(GameObject), true);
            _anchor.IsSupport = EditorGUILayout.Toggle("Is Support", _anchor.IsSupport);
        }

        private static void DrawFocusSetting(FocusResultNotificationDataConfig _focusResult)
        {
            _focusResult.FocusPos =
                EditorGUILayout.Vector3Field("Position", _focusResult.FocusPos);
            _focusResult.FocusRot.eulerAngles =
                EditorGUILayout.Vector4Field("Rotation", _focusResult.FocusRot.eulerAngles);
            _focusResult.FocusState =
                (FindingType) EditorGUILayout.EnumPopup("State",
                    _focusResult.FocusState);
            _focusResult.IsSupport =
                EditorGUILayout.Toggle("Is Support", _focusResult.IsSupport);
            _focusResult.ActionName =
                EditorGUILayout.TextField("Action name", _focusResult.ActionName);
        }
    }
}