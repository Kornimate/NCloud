using NCloud.Users;
using System.ComponentModel.DataAnnotations;

namespace NCloud.Models
{
    public class CloudSpaceRequestViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string SpaceRequest { get; set; } = String.Empty;

        [Required]
        public string RequestJustification { get; set; } = String.Empty;
    }
}
