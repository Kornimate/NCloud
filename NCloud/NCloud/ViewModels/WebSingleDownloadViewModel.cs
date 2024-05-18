using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Web DownloadPage action method
    /// </summary>
    public class WebSingleDownloadViewModel
    {
        public string Path { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}
