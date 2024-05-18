using NCloud.Models;

namespace NCloud.Services
{
    /// <summary>
    /// Interface to manage notifications
    /// </summary>
    public interface ICloudNotificationService
    {
        void AddNotification(CloudNotificationAbstarct notification);
        string GetNotificationQueue();
    }
}
