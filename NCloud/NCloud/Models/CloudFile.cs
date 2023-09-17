namespace NCloud.Models
{
    public class CloudFile : CloudRegistration
    {
        public FileInfo Info { get; set; }
        public CloudFile(FileInfo info, string? icon = null) : base(icon)
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
