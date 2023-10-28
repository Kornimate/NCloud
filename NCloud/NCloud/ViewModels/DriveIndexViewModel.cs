using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveIndexViewModel
    {
        public List<CloudFolder?> recentFolders { get; set; }
        public List<CloudFile?> recentFiles { get; set; }
        public string? TestString { get; set; }

        public DriveIndexViewModel(List<CloudFile?> recFiles,List<CloudFolder?> recFolders)
        {
            recentFiles = recFiles;
            recentFolders = recFolders;
        }
    }
}
