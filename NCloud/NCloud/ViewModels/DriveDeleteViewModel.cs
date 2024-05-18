using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Drive Delete action method
    /// </summary>
    public class DriveDeleteViewModel
    {
        public List<CloudFolder> Folders { get; set; } = null!;
        public List<CloudFile> Files { get; set; } = null!;
        public List<string>? ItemsForDelete { get; set; } = null!;
    }
}
