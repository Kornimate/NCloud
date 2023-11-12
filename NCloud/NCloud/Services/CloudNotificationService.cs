using NCloud.Models;
using NuGet.Protocol;

namespace NCloud.Services
{
    public class CloudNotificationService : ICloudNotificationService
    {
        private readonly List<ACloudNotification> queue;
        public CloudNotificationService()
        {
            queue = new List<ACloudNotification>();
        }

        public void AddNotification(ACloudNotification notification)
        {
            queue.Add(notification);
        }
        public string GetNotificationQueue()
        {
            return queue.OrderBy(x => x.Priority).ToJson();
        }
    }
}
