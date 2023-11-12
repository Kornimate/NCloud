using NCloud.Models;

namespace NCloud.Services
{
    public class CloudNotificationService : ICloudNotificationService
    {
        private readonly PriorityQueue<ACloudNotification, int> queue;
        public CloudNotificationService()
        {
            queue = new PriorityQueue<ACloudNotification, int>();
        }

        public void AddNotification(ACloudNotification notification)
        {
            queue.Enqueue(notification,notification.Priority);
        }
        public PriorityQueue<ACloudNotification,int> GetNotificationQueue()
        {
            return queue;
        }
    }
}
