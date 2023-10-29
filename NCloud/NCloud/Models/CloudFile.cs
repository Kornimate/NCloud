using System.Numerics;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class CloudFile : CloudRegistration
    {
        public FileInfo Info { get; set; }
        public string? Owner { get; set; }
        public CloudFile(FileInfo Info, string? Owner,bool IsShared, string? icon = null) : base(IsShared,icon)
        {
            this.Info = Info;
            this.Owner = Owner;
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
