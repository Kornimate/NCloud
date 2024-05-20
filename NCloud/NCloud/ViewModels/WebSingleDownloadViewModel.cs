using NCloud.Models;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Web DownloadPage action method
    /// </summary>
    public class WebSingleDownloadViewModel
    {
        [Required]
        public string FilePath { get; set; } = null!;

        [Required]
        public string FileName { get; set; } = null!;
    }
}
