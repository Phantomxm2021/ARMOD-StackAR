using System;

namespace com.Phantoms.ActionNotification.Runtime
{
    public class BaseNotificationData:IDisposable
    {
        /// <summary>
        /// 回调
        /// </summary>
        public Action NotificationAct;

        /// <summary>
        /// 该消息的动作类型，用于区分何种消息操作
        /// </summary>
        public string ActionName;

        /// <summary>
        /// 该消息携带的基础数据
        /// </summary>
        public string BaseData;

        public virtual void Dispose()
        {
            NotificationAct = null;
            ActionName = null;
            BaseData = null;
        }
    }
}