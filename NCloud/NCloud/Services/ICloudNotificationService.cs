using NCloud.Models;

namespace NCloud.Services
{
    public interface ICloudNotificationService
    {
        void AddNotification(CloudNotificationAbstarct notification);
        string GetNotificationQueue();
    }
}
