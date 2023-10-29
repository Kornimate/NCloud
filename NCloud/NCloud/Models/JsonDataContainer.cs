using System.Numerics;

namespace NCloud.Models
{
    public class JsonDataContainer
    {
        public string? FolderName { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public Dictionary<string, JsonDetailsContainer>? Folders { get; set; } = new();
        public Dictionary<string, JsonDetailsContainer>? Files { get; set; } = new();
    }
}
