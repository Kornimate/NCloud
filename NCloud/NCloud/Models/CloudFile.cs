namespace NCloud.Models
{
    public class CloudFile : CloudRegistration
    {
        public CloudFile(FileSystemInfo info, string? iconPath = null) : base(info, iconPath) { }

        public override bool IsFile()
        {
            return true;
        }

        public override bool IsFolder()
        {
            return false;
        }
    }
}
