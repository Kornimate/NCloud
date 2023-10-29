using System.Numerics;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class CloudFolder : CloudRegistration
    {
        public DirectoryInfo Info { get; set; }
        public string? Owner { get; set; }
        public CloudFolder(DirectoryInfo Info, string? Owner, bool IsShared, string? icon = null) : base(IsShared, icon)
        {
            this.Info = Info;
            this.Owner = Owner;
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
