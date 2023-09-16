namespace NCloud.Models
{
    public class CloudFile : CloudRegistration
    {
        public FileInfo Info { get; set; }
        public CloudFile(FileInfo info, string? iconPath = null) : base(iconPath)
        {
            Info = info;
        }

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
