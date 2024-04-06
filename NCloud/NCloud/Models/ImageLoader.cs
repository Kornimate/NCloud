using NCloud.ConstantData;
using System.Collections.Generic;

namespace NCloud.Models
{
    public class ImageLoader
    {
        public static string Load(bool isDirectory, string name)
        {
            if (isDirectory)
            {
                return LoadDirectoryIcon();
            }

            return LoadFileIcon(name);
        }
        private static string LoadDirectoryIcon()
        {
            return Constants.FolderIcon;
        }
        private static string LoadFileIcon(string? fileName = null)
        {
            if (fileName == null) { return string.Empty; }

            string extension = Path.GetExtension(fileName).ToLower()?[1..] ?? "general";

            return $"{Constants.PrefixForIcons}{extension}{Constants.SuffixForIcons}";
            //return Constants.IconPaths[extension!]; //uncomment if filetypes added to Contants.IconPaths
        }
    }
}
