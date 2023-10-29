using System.Numerics;

namespace NCloud.Models
{
    public class CloudFolder : CloudRegistration
    {
        public DirectoryInfo Info { get; set; }
        public string? Owner { get; set; }
        public BigInteger Size { get; set; } //in Bytes
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
