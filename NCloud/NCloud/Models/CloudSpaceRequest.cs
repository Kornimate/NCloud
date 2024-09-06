using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCloud.Models
{
    public class CloudSpaceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public SpaceSizes SpaceRequest { get; set; }
        
        public string RequestJustification { get; set; } = String.Empty;
        
        public virtual CloudUser User { get; set; } = null!;
    }
}
