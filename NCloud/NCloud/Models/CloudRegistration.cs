namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public string? SharedName { get; set; }
        public bool IsConnectedToApp { get; set; }
        public bool IsConnectedToWeb { get; set; }
        public CloudRegistration(bool isSharedInApp, bool isPublic, string? fileName = null)
        {
            IconPath = null!;
            IsConnectedToApp = isSharedInApp;
            IsConnectedToWeb = isPublic;
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
        public abstract string ReturnName();
    }
}
