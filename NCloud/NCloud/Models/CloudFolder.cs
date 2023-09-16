namespace NCloud.Models
{
    public class CloudFolder : CloudRegistration
    {
        public DirectoryInfo Info { get; set; }
        public CloudFolder(DirectoryInfo info, string? iconPath = null) : base(iconPath)
        {
            Info = info;
        }

        public override bool IsFile()
        {
            return false;
        }

        public override bool IsFolder()
        {
            return true;
        }
    }
}
