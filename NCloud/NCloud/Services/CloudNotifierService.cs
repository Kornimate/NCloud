using Castle.Core;

namespace NCloud.Services
{
    public enum NotificationType { WARNING, ERROR, INFO, SUCCESS }
    public class CloudNotifierService
    {
        public List<string> Notifications {  get; private set; }
        public CloudNotifierService()
        {
            Notifications = new List<string>();
        }

        public void AddNotification(string text, NotificationType type)
        {
            Notifications.Add(text+"--"+type.ToString());
        }
    }
}
