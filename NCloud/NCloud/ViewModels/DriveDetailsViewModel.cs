using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Drive Details action method
    /// </summary>
    public class DriveDetailsViewModel
    {
        public List<CloudFile> Files { get; set; }
        public List<CloudFolder> Folders { get; set; }
        public string CurrentPathShow { get; set; }
        public string CurrentPath { get; set; }
        public Pair<string, string> WebControllerAndActionForDetails { get; set; }
        public Pair<string, string> WebControllerAndActionForDownload { get; set; }
        public DriveDetailsViewModel(List<CloudFile> files, List<CloudFolder> folders, string currentPathShow, string currentPath, Pair<string, string> webControllerAndActionForDetails, Pair<string, string> webControllerAndActionForDownload)
        {
            Files = files;
            Folders = folders;
            CurrentPathShow = currentPathShow;
            CurrentPath = currentPath;
            WebControllerAndActionForDetails = webControllerAndActionForDetails;
            WebControllerAndActionForDownload = webControllerAndActionForDownload;
        }
    }
}
