using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveDetailsViewModel
    {
        public List<FileInfo?> items { get; set; }
        public string CurrentPath { get; set; }

        public DriveDetailsViewModel(List<FileInfo?> items,string currentPath)
        {
            this.items = items;
            CurrentPath = currentPath;
        }
    }
}
