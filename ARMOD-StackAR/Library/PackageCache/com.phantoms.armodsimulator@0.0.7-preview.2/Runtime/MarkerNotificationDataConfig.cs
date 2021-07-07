using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    [System.Serializable, CreateAssetMenu(menuName = "AR-MOD/Simulator/Notify Config/Marker Notify Data")]
    public class MarkerNotificationDataConfig : BaseNotificationDataConfig
    {
        /// <summary>
        /// 标记名称
        /// </summary>
        public string MarkerName;

        /// <summary>
        /// 追踪标记的状态
        /// </summary>
        public MarkerTrackingState MarkerState;

        /// <summary>
        /// 追踪标记根节点
        /// </summary>
        public Transform MarkerTrackable;

        /// <summary>
        /// 支持状态
        /// </summary>
        public bool IsSupport;
    }
}