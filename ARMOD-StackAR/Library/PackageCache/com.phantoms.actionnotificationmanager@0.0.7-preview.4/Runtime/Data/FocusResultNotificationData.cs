using UnityEngine;

namespace com.Phantoms.ActionNotification.Runtime
{
    /// <summary>
    /// 发现平面状态
    /// </summary>
    public enum FindingType
    {
        Finding,
        Found,
        Limit
    }
    
    public class FocusResultNotificationData : BaseNotificationData
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
    }
}