using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveDetailsViewModel
    {
        public List<CloudFile> Files { get; set; }
        public List<CloudFolder> Folders { get; set; }
        public string CurrentPath { get; set; }
        public DriveDetailsViewModel(List<CloudFile> files, List<CloudFolder> folders, string currentPath)
        {
            Files = files;
            Folders = folders;
            CurrentPath = currentPath;
        }
    }
}
