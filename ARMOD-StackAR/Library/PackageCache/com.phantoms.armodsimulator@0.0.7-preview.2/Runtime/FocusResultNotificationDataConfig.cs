using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    [CreateAssetMenu(menuName = "AR-MOD/Simulator/Notify Config/Focus Notify Data")]

    public class FocusResultNotificationDataConfig : BaseNotificationDataConfig
    {

        /// <summary>
        /// 聚焦位置
        /// </summary>
        public Vector3 FocusPos;

        /// <summary>
        /// 聚焦旋转角度
        /// </summary>
        public Quaternion FocusRot;

        /// <summary>
        /// 聚焦发现平面状态
        /// </summary>
        public FindingType FocusState;

        /// <summary>
        /// 支持状态
        /// </summary>
        public bool IsSupport;
    }
}