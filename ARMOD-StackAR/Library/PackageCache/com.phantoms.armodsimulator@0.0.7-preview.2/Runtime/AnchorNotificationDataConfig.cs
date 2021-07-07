using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    [System.Serializable, CreateAssetMenu(menuName = "AR-MOD/Simulator/Notify Config/Anchor Notify Data")]
    public class AnchorNotificationDataConfig : BaseNotificationDataConfig
    {
        public AnchorNotificationData.StickTypeEnum StickType;
        public AnchorNotificationData.TrackableTypeEnum TrackableType;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Offset;
        public GameObject ControllerTargetNode;


        /// <summary>
        /// 支持状态
        /// </summary>
        public bool IsSupport;
    }
}