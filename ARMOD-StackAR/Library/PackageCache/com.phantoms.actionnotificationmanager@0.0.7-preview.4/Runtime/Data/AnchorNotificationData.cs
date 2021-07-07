using System;
using UnityEngine;

namespace com.Phantoms.ActionNotification.Runtime
{
    public class AnchorNotificationData : BaseNotificationData
    {
        [Flags]
        public enum TrackableTypeEnum:int
        {
            None = 0,
            PlaneWithinPolygon = 1,
            PlaneWithinBounds = 2,
            PlaneWithinInfinity = 4,
            PlaneEstimated = 8,
            Planes = PlaneEstimated | PlaneWithinInfinity | PlaneWithinBounds | PlaneWithinPolygon, // 0x0000000F
            FeaturePoint = 16, // 0x00000010
            Image = 32, // 0x00000020
            Face = 64, // 0x00000040
            All = Face | Image | FeaturePoint | Planes, // 0x0000007F
        }

        public StickTypeEnum StickType;
        public TrackableTypeEnum TrackableType;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Offset;
        public GameObject ControllerTargetNode;

        public enum StickTypeEnum
        {
            ByScreen,
            ByTrackableType
        }
    }
}