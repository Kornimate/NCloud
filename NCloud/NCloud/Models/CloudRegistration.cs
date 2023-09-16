namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public CloudRegistration(string? iconPath = null)
        {
            IconPath = iconPath;
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
    }
}
