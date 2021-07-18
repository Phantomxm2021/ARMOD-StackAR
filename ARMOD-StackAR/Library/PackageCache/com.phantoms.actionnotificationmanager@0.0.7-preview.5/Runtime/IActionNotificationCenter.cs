namespace com.Phantoms.ActionNotification.Runtime
{
    public interface IActionNotificationCenter<in T>
    {
        void AddObserver(T _action, string _name);
        void RemoveObserver(string _name, T _action);
        void RemoveObserver(string _name);
        void PostNotification(string _name, BaseNotificationData _notificationData);
        
    }
}