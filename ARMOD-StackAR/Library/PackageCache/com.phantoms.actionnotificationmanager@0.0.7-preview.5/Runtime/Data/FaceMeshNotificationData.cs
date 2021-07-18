using Unity.Collections;
using UnityEngine;

namespace com.Phantoms.ActionNotification.Runtime
{
    public enum FaceTrackingState
    {
        /// <summary>
        /// Not tracking.
        /// </summary>
        None,

        /// <summary>
        /// Some tracking information is available, but it is limited or of poor quality.
        /// </summary>
        Limited,

        /// <summary>
        /// Tracking is working normally.
        /// </summary>
        Tracking,
    }
    public class FaceMeshNotificationData:BaseNotificationData
    {
        public string TrackingId;
        public GameObject FaceGameObject;
        public FaceTrackingState FaceTrackingState;
        public NativeArray<Vector3> vertices;
        public NativeArray<int> indices;
        public NativeArray<Vector3> normals;
        public NativeArray<Vector2> uvs;
    }
}