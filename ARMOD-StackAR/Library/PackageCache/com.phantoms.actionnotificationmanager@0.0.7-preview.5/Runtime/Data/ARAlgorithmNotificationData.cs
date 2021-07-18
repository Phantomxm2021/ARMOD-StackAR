using System.Collections.Generic;

namespace com.Phantoms.ActionNotification.Runtime
{
    public enum ARAlgorithmType
    {
        Anchor,
        FocusSLAM,
        ImageTracker,
        Immersal,
        FaceMesh
    }

    public enum ARAlgorithmOperator
    {
        StartAlgorithm,
        PauseAlgorithm,
        StopAlgorithm
    }

    public class ARAlgorithmNotificationData : BaseNotificationData
    {
        public ARAlgorithmType ARAlgorithmType;
        public ARAlgorithmOperator ARAlgorithmOperator;
        public bool Mixed;
    }
}