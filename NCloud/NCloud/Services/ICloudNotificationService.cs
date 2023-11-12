using NCloud.Models;

namespace NCloud.Services
{
    public interface ICloudNotificationService
    {
        void AddNotification(ACloudNotification notification);
        string GetNotificationQueue();
    }
}
