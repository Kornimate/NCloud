using NCloud.ConstantData;

namespace NCloud.Services
{
    /// <summary>
    /// Class to create file path to icons
    /// </summary>
    public class IconManager
    {
        /// <summary>
        /// Static method to load icon for file system item (file/folder)
        /// </summary>
        /// <param name="isDirectory">Boolean if item is folder</param>
        /// <param name="name">Name of item</param>
        /// <returns>The created path to the specified icon</returns>
        public static string Load(bool isDirectory, string name)
        {
            if (isDirectory)
            {
                return LoadDirectoryIcon();
            }

            return LoadFileIcon(name);
        }

        /// <summary>
        /// Static method to load folder icon
        /// </summary>
        /// <returns>The folder icon</returns>
        private static string LoadDirectoryIcon()
        {
            return Constants.FolderIcon;
        }

        /// <summary>
        /// Static method to load icon for file related to its file type
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>The path to the file type icon</returns>
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
