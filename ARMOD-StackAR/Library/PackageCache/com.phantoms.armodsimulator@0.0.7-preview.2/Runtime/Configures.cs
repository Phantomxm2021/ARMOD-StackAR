using System;
using System.Collections.Generic;
using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    public class Configures
    {
        public AlgorithmType Algorithm;
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
        public Vector3 OnScreenPositionOffset;
        public ImageLostMode ImageLostMode;
        public int MaxMovingOfTracking;

        //features
        public bool EnabledFeatures;
        public bool EnvironmentProbe;
        public bool LightEstimation;
        public bool CoachingOverlay;
        public bool PostProcessing;
        public bool CanInteraction;
        public bool AutoSelect;

        //immersal
        public List<string> Maps = new List<string>();
        public string DeveloperToken;
    }


    public enum AlgorithmType
    {
        FocusSlam,
        Anchor,
        Gyro,
        Fixed,
        ImageTracker,
        Immersal
    }

    public enum ImageLostMode
    {
        Hiding,
        Destroy,
        OnScreen,
        Custom
    }

    [Flags]
    public enum PlaneDetectionMode
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
    }
}