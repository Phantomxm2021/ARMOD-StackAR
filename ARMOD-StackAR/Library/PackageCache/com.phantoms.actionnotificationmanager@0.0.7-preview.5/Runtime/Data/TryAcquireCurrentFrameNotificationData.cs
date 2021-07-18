using UnityEngine;

namespace com.Phantoms.ActionNotification.Runtime
{
    public class TryAcquireCurrentFrameNotificationData : BaseNotificationData
    {
        public TextureFormat AcquiredTextureFormat;
        public ConversionType ConversionType;
    }

    public enum ConversionType
    {
        Synchronous,
        Asynchronous
    }
}