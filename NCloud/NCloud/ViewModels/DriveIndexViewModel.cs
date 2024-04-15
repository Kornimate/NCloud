using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveIndexViewModel
    {
        public List<CloudFolder?> recentFolders { get; set; }
        public List<CloudFile?> recentFiles { get; set; }
        public List<string> SharedFoldersUrls { get; set; }
        public List<string> SharedFilesUrls { get; set; }

        public DriveIndexViewModel(List<CloudFile?> recFiles,List<CloudFolder?> recFolders, List<string> sharedFoldersUrls, List<string> sharedFilesUrls)
        {
            recentFiles = recFiles;
            recentFolders = recFolders;
            SharedFoldersUrls = sharedFoldersUrls;
            SharedFilesUrls = sharedFilesUrls;
        }
    }
}
