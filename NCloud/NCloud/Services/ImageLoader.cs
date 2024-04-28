using NCloud.ConstantData;
using System.Collections.Generic;

namespace NCloud.Services
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

            string extensionFilter = Path.GetExtension(fileName).ToLower();

            string extension = extensionFilter != string.Empty ? extensionFilter[1..] : Constants.NoFileType;

            if (File.Exists(Path.Combine(Constants.IconsBasePath, $"{Constants.FileTypePrefix}{extension}{Constants.SuffixForIcons}")))
            {
                return $"{Constants.PrefixForIcons}{extension}{Constants.SuffixForIcons}";
            }

            return $"{Constants.PrefixForIcons}{Constants.UnkownFileType}{Constants.SuffixForIcons}";
        }
    }
}
