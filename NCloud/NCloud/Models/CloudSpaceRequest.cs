using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCloud.Models
{
    /// <summary>
    /// Class to store space request in database
    /// </summary>
    public class CloudSpaceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public SpaceSizes SpaceRequest { get; set; }
        
        public string RequestJustification { get; set; } = String.Empty;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual CloudUser User { get; set; } = null!;
    }
}
