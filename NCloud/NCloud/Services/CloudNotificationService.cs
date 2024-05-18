using NCloud.Models;
using NuGet.Protocol;

namespace NCloud.Services
{
    /// <summary>
    /// Class to handle notification adds from services and present it on UI
    /// </summary>
    public class CloudNotificationService : ICloudNotificationService
    {
        private readonly List<CloudNotificationAbstarct> queue;
        public CloudNotificationService()
        {
            queue = new List<CloudNotificationAbstarct>();
        }

        /// <summary>
        /// Method to add new notification to the collection of notifications
        /// </summary>
        /// <param name="notification">The notification to be added</param>
        public void AddNotification(CloudNotificationAbstarct notification)
        {
            queue.Add(notification);
        }

        /// <summary>
        /// Gets the notifications from collection and creates JSON from them to present on UI
        /// </summary>
        /// <returns>The JSON formatted string of notifications</returns>
        public string GetNotificationQueue()
        {
            return queue.OrderBy(x => x.Priority).ToJson();
        }
    }
}
