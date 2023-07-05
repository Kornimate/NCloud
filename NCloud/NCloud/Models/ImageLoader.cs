using System.Collections.Generic;

namespace NCloud.Models
{
    public class ImageLoader
    {
        public static string Load(string? fileName = null)
        {
            if (Path.GetExtension(fileName) == null)
            {
                return iconPaths["Folder"];
            }
            else
            {
                return iconPaths["File"]; // @todo, get file icons and implement them to this
            }
        }

        private static Dictionary<string, string> iconPaths = new Dictionary<string, string>()
        {
            {"Folder",@"/utilities/folder.svg" },
            {"File",@"/utilities/file.svg" }
        };
    }
}
