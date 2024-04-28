using System.Numerics;
using System.Text.Json.Serialization;
using NCloud.Services;

namespace NCloud.Models
{
    public class CloudFolder : CloudRegistration
    {
        public DirectoryInfo Info { get; set; }
        public CloudFolder(DirectoryInfo Info, bool isSharedInApp, bool isPublic, string currentPath, string? icon = null) : base(isSharedInApp, isPublic, currentPath)
        {
            this.Info = Info;
            this.IconPath = icon is null ? IconManager.Load(IsFolder(), Info.Name) : icon;
        }
        public CloudFolder(string sharedName, string? icon = null) : base(true, false, icon)
        {
            SharedName = sharedName;
            this.IconPath = icon is null ? IconManager.Load(IsFolder(), sharedName) : icon;
            Info = null!;
        }

        public override bool IsFile()
        {
            return false;
        }

        public override bool IsFolder()
        {
            return true;
        }

        public override string ReturnName()
        {
            return Info.Name;
        }
    }
}
