namespace com.Phantoms.ActionNotification.Runtime
{
    public enum OcclusionDataType
    {
        HumanStencil,
        HumanDepth,
        Environment,
        EnvironmentConfidence
    }

    public class AROcclusionNotificationData : BaseNotificationData
    {
        public OcclusionDataType OcclusionDataType;
    }
}