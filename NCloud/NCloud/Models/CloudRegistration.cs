namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public bool IsSharedInApp { get; set; }
        public bool IsPublic { get; set; }
        public CloudRegistration(bool isSharedInApp, bool isPublic, string? fileName = null)
        {
            IconPath = null!;
            IsSharedInApp = isSharedInApp;
            IsPublic = isPublic;
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
        public abstract string ReturnName();
    }
}
