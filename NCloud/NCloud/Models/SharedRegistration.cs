using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace NCloud.Models
{
    public abstract class Sharedregistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [NotNull]
        public string? Name { get; set; }

        [Required]
        [NotNull]
        public string? PathFromRoot { get; set; }

        [Required]
        [NotNull]
        public virtual CloudUser? Owner { get; set; }

        public virtual bool IsFile() { return false; }
        public virtual bool IsFolder() { return false; }
    }
}
