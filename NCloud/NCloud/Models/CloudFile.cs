using System.Text;
using NCloud.ConstantData;
using NCloud.Services;

namespace NCloud.Models
{
    /// <summary>
    /// Class to wrap information about physical files on disk
    /// </summary>
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

        /// <summary>
        /// Method to return that object is file
        /// </summary>
        /// <returns>True if file, otherwise false</returns>
        public override bool IsFile()
        {
            return true;
        }

        /// <summary>
        /// Method to return that object is folder
        /// </summary>
        /// <returns>True if folder, otherwise false</returns>
        public override bool IsFolder()
        {
            return false;
        }

        /// <summary>
        /// Method to return file name
        /// </summary>
        /// <returns>The name of the file</returns>
        public override string ReturnName()
        {
            return Info.Name;
        }

        /// <summary>
        /// Method to create a string representation of file
        /// </summary>
        /// <returns>The file info in a formatted string</returns>
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
            sb.Append(IsConnectedToWeb ? "yes".PadRight(13) : "no".PadRight(13));
            sb.Append("".PadRight(2));
            sb.Append(Info.Name);
            sb.Append('\n');

            return sb.ToString();
        }
    }
}
