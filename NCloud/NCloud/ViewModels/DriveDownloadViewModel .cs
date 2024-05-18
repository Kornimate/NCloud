using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Drive DownloadItems action method
    /// </summary>
    public class DriveDownloadViewModel
    {
        public List<CloudFolder> Folders { get; set; } = null!;
        public List<CloudFile> Files { get; set; } = null!;
        public List<string>? ItemsForDownload { get; set; } = null!;
    }
}
