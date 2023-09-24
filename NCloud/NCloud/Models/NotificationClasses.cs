namespace NCloud.Models
{
    public interface INotification { }

    public abstract class NotificationAbstract
    {
        public string Text { get; private set; }
        public string Color { get; private set; }
        public string Title { get; private set; }

        protected NotificationAbstract(string text, string title,string color)
        {
            Text = text;
            Title = title;
            Color = color;
        }
    }

    public class NotificationOK : NotificationAbstract
    {
        public NotificationOK(string title, string text) : base(text, title, "bg-success") { }
    }

    public class NotificationWarning : NotificationAbstract
    {
        public NotificationWarning(string title, string text) : base(text, title, "bg-warning") { }
    }

    public class NotificationFail : NotificationAbstract
    {
        public NotificationFail(string title, string text) : base(text, title, "bg-danger") { }
    }

    public class NotificationInfo : NotificationAbstract
    {
        public NotificationInfo(string title, string text) : base(text, title, "bg-primary") { }
    }
}
