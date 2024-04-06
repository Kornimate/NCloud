using System.Numerics;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class CloudFile : CloudRegistration
    {
        public FileInfo Info { get; set; }
        public CloudFile(FileInfo Info, bool IsSharedInApp, bool isPublic, string? icon = null) : base(IsSharedInApp, isPublic, icon)
        {
            this.Info = Info;
            this.IconPath = ImageLoader.Load(IsFolder(), Info.Name);
        }

        public override bool IsFile()
        {
            return true;
        }

        public override bool IsFolder()
        {
            return false;
        }

        public override string ReturnName()
        {
            return Info.Name;
        }
    }
}
