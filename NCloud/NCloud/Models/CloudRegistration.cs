namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public FileSystemInfo? Info { get; set; }
        public string? IconPath { get; set; }
        public CloudRegistration(FileSystemInfo info, string? iconPath = null)
        {
            Info = info;
            IconPath = iconPath;
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
    }
}
