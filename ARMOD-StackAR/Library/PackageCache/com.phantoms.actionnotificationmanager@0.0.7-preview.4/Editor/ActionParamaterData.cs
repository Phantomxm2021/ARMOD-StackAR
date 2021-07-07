using System.Collections.Generic;
using UnityEngine;

namespace Action_Notification_Manager.Editor
{
    //[CreateAssetMenu(menuName = "AR-MOD/ANM/ActionKey")]
    public class ActionParamaterData : ScriptableObject
    {
        public List<string> ObserverKeys = new List<string>();
    }
}