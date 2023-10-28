using System.Numerics;

namespace NCloud.Models
{
    public class JsonDataContainer
    {
        public string? FolderName { get; set; }
        public bool IsShared { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Owner { get; set; }
        public BigInteger Size { get; set; } //in Bytes
        public List<CloudRegistration>? Items { get; set; }
    }
}
