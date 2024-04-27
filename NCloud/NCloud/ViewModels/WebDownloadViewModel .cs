using NCloud.Models;

namespace NCloud.ViewModels
{
    public class WebDownloadViewModel
    {
        public List<CloudFolder> Folders { get; set; } = null!;
        public List<CloudFile> Files { get; set; } = null!;
        public List<string>? ItemsForDownload { get; set; } = null!;
        public string Path { get; set; } = null!;
    }
}
