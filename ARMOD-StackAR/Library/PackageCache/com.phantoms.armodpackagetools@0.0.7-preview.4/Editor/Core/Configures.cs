using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class Configures : ScriptableObject
    {
        public float ARWorldScale = 1.0f;
        public string Version = "0.0.1";
        public AlgorithmType Algorithm;
        public bool AlgorithmAutoStart = true;

        public PlaneDetectionMode PlaneDetectionMode = PlaneDetectionMode.Horizontal;
        public string ProjectName;

        //script
        public string MainEntry;
        public string DomainName;
        public bool DebugModel;

        //visualizer
        public string CustomPointCloudVisualizerName;
        public string CustomPlaneVisualizerName;
        public bool AutoPlacementOfEnvironmentProbe;

        //2d features
        public int MaxMovingOfTracking;

        //features
        public bool EnvironmentProbe;
        public bool LightEstimation;
        public bool CoachingOverlay;
        public bool PostProcessing;
        public bool CanInteraction;
        public bool AutoSelect;
        public bool AROcclusion;

        //face mesh
        public int MaximumFaceCount = 1;

        //immersal
        public string DeveloperToken;
        public float LocalizationInterval;
        public bool UseServerLocalizer;
        public RenderMode RenderMode;
        public bool TurnOffServerLocalizedAfterSuccess = true;

        //Occlusion
        public EnvironmentDepthMode EnvironmentDepthMode;
        public HumanSegmentationDepthMode HumanSegmentationDepthMode;
        public HumanSegmentationStencilMode HumanSegmentationStencilMode;
        public OcclusionPreferenceMode OcclusionPreferenceMode;
        
        
        //Graphics
        public int QualityLevel=1;
    }

    public enum RenderMode : int
    {
        DoNotRender,
        EditorOnly,
        EditorAndRuntime
    }

    public enum AlgorithmType
    {
        FocusSlam,
        Anchor,
        Gyro,
        Fixed,
        ImageTracker,
        Immersal,
        FaceMesh
    }

    public enum ImageLostMode
    {
        Hiding,
        Destroy,
        OnScreen,
        Custom
    }
}