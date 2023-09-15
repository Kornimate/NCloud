using NCloud.Models;
using FileII = NCloud.Models.FileII;

namespace NCloud.ViewModels
{
    public class DriveIndexViewModel
    {
        public List<FolderII?> recentFolders { get; set; }
        public List<FileII?> recentFiles { get; set; }

        public DriveIndexViewModel(List<FileII?> recFiles,List<FolderII?> recFolders)
        {
            recentFiles = recFiles;
            recentFolders = recFolders;
        }
    }
}
