using Newtonsoft.Json;

namespace NCloud.Models
{
    /// <summary>
    /// Abstract class to handle notifications presented for the user (works as base class for differernt notifications)
    /// </summary>
    public class CloudNotificationAbstarct
    {
        public string? Color { get; private set; }
        public string? Image { get; private set; }
        public string? Title { get; private set; }
        public string? Text { get; private set; }
        public int Priority { get; private set; }

        [JsonConstructor]
        public CloudNotificationAbstarct(string? color, string? image, string? title, int priority, string? text)
        {
            Color = color;
            Image = image;
            Title = title;
            Text = text;
            Priority = priority;
        }
    }
}
