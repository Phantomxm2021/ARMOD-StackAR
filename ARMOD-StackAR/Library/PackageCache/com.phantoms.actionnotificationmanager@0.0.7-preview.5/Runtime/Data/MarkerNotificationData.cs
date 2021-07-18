using UnityEngine;

namespace com.Phantoms.ActionNotification.Runtime
{
    public enum MarkerTrackingState : int
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

        /// <summary>
        /// added since the last event.
        /// </summary>
        Added,
    }

    public class MarkerNotificationData : BaseNotificationData
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

        public override void Dispose()
        {
            base.Dispose();

            if (MarkerTrackable)
                Object.DestroyImmediate(MarkerTrackable, true);

            MarkerName = null;
        }
    }
}