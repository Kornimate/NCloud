using System.Text;
using NCloud.ConstantData;
using NCloud.Services;

namespace NCloud.Models
{
    public class CloudFile : CloudRegistration
    {
        public FileInfo Info { get; set; }
        public CloudFile(FileInfo Info, bool IsSharedInApp, bool isPublic, string currentPath, string? icon = null) : base(IsSharedInApp, isPublic, currentPath)
        {
            this.Info = Info;
            this.IconPath = icon is null ? IconManager.Load(IsFolder(), Info.Name) : icon;
        }
        public CloudFile(string sharedName, string? icon = null) : base(true, false, String.Empty)
        {
            SharedName = sharedName;
            this.IconPath = icon is null ? IconManager.Load(IsFolder(), sharedName) : icon;
            Info = null!;
        }

        public CloudFile(string itemPath) : base(false, false, String.Empty)
        {
            ItemPath = itemPath;
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

        public override string ToString()
        {
            if (Info is null)
                return "No information available";

            StringBuilder sb = new StringBuilder();

            sb.Append(Info.CreationTime.ToString(Constants.TerminalDateTimeFormat));
            sb.Append("".PadRight(2));
            sb.Append(FileSizeManager.ConvertToReadableSize(Info.Length).PadRight(10));
            sb.Append("".PadRight(2));
            sb.Append(IsConnectedToApp ? "yes".PadRight(13) : "no".PadRight(13));
            sb.Append("".PadRight(2));
            sb.Append(IsConnectedToApp ? "yes".PadRight(13) : "no".PadRight(13));
            sb.Append("".PadRight(2));
            sb.Append(Info.Name);
            sb.Append('\n');

            return sb.ToString();
        }
    }
}
