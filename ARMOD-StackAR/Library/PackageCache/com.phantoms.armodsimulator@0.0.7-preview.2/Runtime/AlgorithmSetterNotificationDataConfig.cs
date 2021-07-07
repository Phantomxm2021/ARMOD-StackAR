using UnityEngine;

namespace  com.Phantoms.ARMODSimulator.Runtime
{
    [System.Serializable,CreateAssetMenu(menuName = "AR-MOD/Simulator/Notify Config/Algorithm Setter Notify Data")]
    public class AlgorithmSetterNotificationDataConfig:BaseNotificationDataConfig
    {
        public bool AlgorithmState;

    }
}