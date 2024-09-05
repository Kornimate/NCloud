using NCloud.Users;

namespace NCloud.Models
{
    public class CloudSpaceRequest
    {
        public Guid Id { get; set; }

        public SpaceSizes SpaceRequest { get; set; }
        
        public string RequestJustification { get; set; } = String.Empty;
        
        public virtual CloudUser User { get; set; } = null!;
    }
}
