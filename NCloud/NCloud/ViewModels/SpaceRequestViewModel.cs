using NCloud.Users;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class SpaceRequestViewModel
    {
        [Required]
        public string SpaceRequest { get; set; } = String.Empty;

        [Required]
        public string RequestJustification { get; set; } = String.Empty;

        public CloudUser? User { get; set; }
    }
}
