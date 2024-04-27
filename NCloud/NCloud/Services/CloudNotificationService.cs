using NCloud.Models;
using NuGet.Protocol;

namespace NCloud.Services
{
    public class CloudNotificationService : ICloudNotificationService
    {
        private readonly List<CloudNotificationAbstarct> queue;
        public CloudNotificationService()
        {
            queue = new List<CloudNotificationAbstarct>();
        }

        public void AddNotification(CloudNotificationAbstarct notification)
        {
            queue.Add(notification);
        }
        public string GetNotificationQueue()
        {
            return queue.OrderBy(x => x.Priority).ToJson();
        }
    }
}
