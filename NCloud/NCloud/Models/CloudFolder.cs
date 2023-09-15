namespace NCloud.Models
{
    public class CloudFolder : CloudRegistration
    {
        public CloudFolder(FileSystemInfo info, string? iconPath = null) : base(info, iconPath) { }

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
