using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Web DownloadItems action method
    /// </summary>
    public class WebDownloadViewModel
    {
        public List<CloudFolder> Folders { get; set; } = null!;
        public List<CloudFile> Files { get; set; } = null!;
        public List<string>? ItemsForDownload { get; set; } = null!;
        public string Path { get; set; } = null!;
    }
}
