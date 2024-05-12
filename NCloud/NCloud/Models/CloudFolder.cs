using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using NCloud.ConstantData;
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

        public CloudFolder(string itemPath) : base(false, false, String.Empty)
        {
            ItemPath = itemPath;
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

        public override string ToString()
        {
            if (Info is null)
                return "No information available";

            StringBuilder sb = new StringBuilder();

            sb.Append(Info.CreationTime.ToString(Constants.TerminalDateTimeFormat));
            sb.Append("".PadRight(14));
            sb.Append(IsConnectedToApp ? "yes".PadRight(13) : "no".PadRight(13));
            sb.Append("".PadRight(2));
            sb.Append(IsConnectedToWeb? "yes".PadRight(13) : "no".PadRight(13));
            sb.Append("".PadRight(2));
            sb.Append(Info.Name);
            sb.Append('\n');

            return sb.ToString();
        }
    }
}
