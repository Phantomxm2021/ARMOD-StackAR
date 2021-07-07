using System;
using UnityEngine;
using System.Collections.Generic;

namespace com.Phantoms.ActionNotification.Runtime
{
    public class ActionNotificationCenter : MonoBehaviour, IActionNotificationCenter<Action<BaseNotificationData>>
    {
        private static ActionNotificationCenter DEFAULT_CENTER;

        public static ActionNotificationCenter DefaultCenter
        {
            get
            {
                if (DEFAULT_CENTER) return DEFAULT_CENTER;
                GameObject tmp_NotificationObject = new GameObject("Notification Center");
                DEFAULT_CENTER = tmp_NotificationObject.AddComponent<ActionNotificationCenter>();
                DontDestroyOnLoad(tmp_NotificationObject);
                return DEFAULT_CENTER;
            }
        }

        private readonly Dictionary<string, List<Action<BaseNotificationData>>> notifications =
            new Dictionary<string, List<Action<BaseNotificationData>>>();

        private readonly Dictionary<string, List<Func<BaseNotificationData, object>>> notificationsResults =
            new Dictionary<string, List<Func<BaseNotificationData, object>>>();


        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                this.enabled = false;
                DestroyImmediate(this, true);
            }
#endif
        }

        public void AddObserver(Action<BaseNotificationData> _action, string _name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                Debug.LogError("Null name specified for notification in AddObserver.");
                return;
            }

            if (!notifications.ContainsKey(_name))
            {
                notifications.Add(_name, new List<Action<BaseNotificationData>>());
            }


            if (notifications[_name] is { } tmp_NotifyList
                && !tmp_NotifyList.Contains(_action))
            {
                tmp_NotifyList.Add(_action);
            }
        }


        public void AddObserver(Func<BaseNotificationData, object> _action, string _name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                Debug.LogError("Null name specified for notification in AddObserver.");
                return;
            }

            if (!notificationsResults.ContainsKey(_name))
            {
                notificationsResults.Add(_name, new List<Func<BaseNotificationData, object>>());
            }


            if (notificationsResults[_name] is { } tmp_NotifyList
                && !tmp_NotifyList.Contains(_action))
            {
                tmp_NotifyList.Add(_action);
            }
        }


        public void RemoveObserver(string _name, Action<BaseNotificationData> _action)
        {
            if (!notifications.ContainsKey(_name)) return;
            if (!(notifications[_name] is { } tmp_NotifyList)) return;
            if (tmp_NotifyList.Contains(_action)) tmp_NotifyList.Remove(_action);
            if (tmp_NotifyList.Count == 0) notifications.Remove(_name);
        }

        public void RemoveObserver(string _name, Func<BaseNotificationData, object> _action)
        {
            if (!notificationsResults.ContainsKey(_name)) return;
            if (!(notificationsResults[_name] is { } tmp_NotifyList)) return;
            if (tmp_NotifyList.Contains(_action)) tmp_NotifyList.Remove(_action);
            if (tmp_NotifyList.Count == 0) notificationsResults.Remove(_name);
        }

        public void RemoveObserver(string _name)
        {
            if (notifications.ContainsKey(_name)) notifications.Remove(_name);
            if (notificationsResults.ContainsKey(_name)) notificationsResults.Remove(_name);
        }


        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="_name">被通知的方法</param>
        /// <param name="_object">通知的参数数据</param>
        public void PostNotification(string _name, BaseNotificationData _object)
        {
            if (string.IsNullOrEmpty(_name))
            {
#if DEBUG
                Debug.LogError("Null name sent to PostNotification.");
#endif
                return;
            }

            if (!notifications.ContainsKey(_name))
            {
#if DEBUG
                Debug.LogError($"{_name} key is no in Notification center!");
#endif
                return;
            }

            if (!(notifications[_name] is { } tmp_NotifyList)) return;
            foreach (Action<BaseNotificationData> tmp_Action in tmp_NotifyList)
            {
                tmp_Action?.Invoke(_object);
            }
        }


        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="_name">被通知的方法</param>
        /// <param name="_object">通知的参数数据</param>
        public List<object> PostNotificationWithResult(string _name, BaseNotificationData _object)
        {
            if (string.IsNullOrEmpty(_name))
            {
#if DEBUG
                Debug.LogError("Null name sent to PostNotification.");
#endif
                return null;
            }

            if (!notificationsResults.ContainsKey(_name))
            {
#if DEBUG
                Debug.LogError($"{_name} key is no in Notification center!");
#endif
                return null;
            }

            if (!(notificationsResults[_name] is { } tmp_NotifyList)) return null;
            List<object> tmp_Results = new List<object>();
            foreach (Func<BaseNotificationData, object> tmp_Action in tmp_NotifyList)
            {
                tmp_Results.Add(tmp_Action?.Invoke(_object));
            }

            return tmp_Results;
        }
    }
}