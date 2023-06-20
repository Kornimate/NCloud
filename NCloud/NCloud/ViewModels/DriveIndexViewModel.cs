using NCloud.Models;
using File = NCloud.Models.File;

namespace NCloud.ViewModels
{
    public class DriveIndexViewModel
    {
        public List<Folder?> recentFolders { get; set; }
        public List<File?> recentFiles { get; set; }

        public DriveIndexViewModel(List<File?> recFiles,List<Folder?> recFolders)
        {
            recentFiles = recFiles;
            recentFolders = recFolders;
        }
    }
}
