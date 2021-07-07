using System;
using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    [System.Serializable,CreateAssetMenu(menuName = "AR-MOD/Simulator/Notify Config/Base Notify Data")]
    public class BaseNotificationDataConfig : ScriptableObject
    {
        /// <summary>
        /// 回调
        /// </summary>
        public Action notificationAct;

        /// <summary>
        /// 该消息的动作类型，用于区分何种消息操作
        /// </summary>
        public string ActionName;
    }
}