namespace NCloud.Models
{
    public class CloudFolder : CloudRegistration
    {
        public DirectoryInfo Info { get; set; }
        public CloudFolder(DirectoryInfo info, string? icon = null) : base(icon)
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
