using System.Numerics;

namespace NCloud.Models
{
    public class JsonDataContainer
    {
        public string? FolderName { get; set; }
        public bool IsShared { get; set; } = false;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public List<CloudFolder>? Folders { get; set; } = new List<CloudFolder>();
        public List<CloudFile>? Files { get; set; } = new List<CloudFile>();
    }
}
