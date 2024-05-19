using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using NCloud.ConstantData;
using NCloud.Services;

namespace NCloud.Models
{
    /// <summary>
    /// Class to wrap information about physical folders on disk
    /// </summary>
    public class CloudFolder : CloudRegistration
    {
        public DirectoryInfo Info { get; set; }
        public CloudFolder(DirectoryInfo Info, bool isSharedInApp, bool isPublic, string currentPath, string? itemPath = null) : base(isSharedInApp, isPublic, currentPath)
        {
            this.Info = Info;
            this.IconPath = IconManager.Load(IsFolder(), Info.Name);
            ItemPath = itemPath ?? String.Empty;
        }
        public CloudFolder(string sharedName, string? icon = null) : base(true, false, icon)
        {
            SharedName = sharedName;
            this.IconPath = IconManager.Load(IsFolder(), sharedName);
            Info = null!;
        }

        public CloudFolder(string itemPath) : base(false, false, String.Empty)
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
            return false;
        }

        /// <summary>
        /// Method to return that object is folder
        /// </summary>
        /// <returns>True if folder, otherwise false</returns>
        public override bool IsFolder()
        {
            return true;
        }

        /// <summary>
        /// Method to return folder name
        /// </summary>
        /// <returns>The name of the folder</returns>
        public override string ReturnName()
        {
            return Info.Name;
        }
        /// <summary>
        /// Method to create a string representation of folder
        /// </summary>
        /// <returns>The folder info in a formatted string</returns>
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
