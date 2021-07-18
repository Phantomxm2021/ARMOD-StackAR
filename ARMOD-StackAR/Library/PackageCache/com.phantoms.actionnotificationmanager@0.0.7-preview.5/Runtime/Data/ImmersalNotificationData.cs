using UnityEngine;

namespace com.Phantoms.ActionNotification.Runtime
{
    public struct LocalizerPose
    {
        public bool valid;
        public double[] mapToEcef;
        public Matrix4x4 matrix;
        public Pose lastUpdatedPose;
        public double vLatitude;
        public double vLongitude;
        public double vAltitude;
    }
    public class ImmersalNotificationData : BaseNotificationData
    {
        public int MapId;
        public LocalizerPose LocalizerPose;
    }
}