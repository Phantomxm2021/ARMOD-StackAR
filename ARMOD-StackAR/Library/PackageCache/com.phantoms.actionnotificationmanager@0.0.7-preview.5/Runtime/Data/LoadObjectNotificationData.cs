using System;
using System.Collections.Generic;

namespace com.Phantoms.ActionNotification.Runtime
{
    public class LoadObjectNotificationData:BaseNotificationData
    {
        public string ProjectName;
        public List<string> LoadObjectName;
        public Type LoadObjectType;
    }
}