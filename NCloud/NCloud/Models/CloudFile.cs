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
            this.IconPath = icon is null ? ImageLoader.Load(IsFolder(), Info.Name) : icon;
        }
        public CloudFile(string sharedName, string? icon = null) : base(true, false, icon)
        {
            SharedName = sharedName;
            this.IconPath = icon is null ? ImageLoader.Load(IsFolder(), sharedName) : icon;
            Info = null!;
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
