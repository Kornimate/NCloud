using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveDeleteViewModel
    {
        public List<CloudFolder?> Folders { get; set; } = null!;
        public List<CloudFile?> Files { get; set; } = null!;
        public List<string>? ItemsForDelete { get; set; } = null!;
    }
}
