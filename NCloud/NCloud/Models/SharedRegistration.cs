using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace NCloud.Models
{
    /// <summary>
    /// Abstract class to create database schema for shared items
    /// </summary>
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
        public string? SharedPathFromRoot { get; set; }

        [Required]
        [NotNull]
        public string? CloudPathFromRoot { get; set; }

        [Required]
        [NotNull]
        public virtual CloudUser? Owner { get; set; }

        [Required]
        [NotNull]
        public bool ConnectedToWeb { get; set; }

        [Required]
        [NotNull]
        public bool ConnectedToApp { get; set; }

        /// <summary>
        /// Virtual method to indicate that object is file
        /// </summary>
        /// <returns>True if object if file, otherwise false</returns>
        public virtual bool IsFile() { return false; }

        /// <summary>
        /// Virtual method to indicate that object is folder
        /// </summary>
        /// <returns>True if object if folder, otherwise false</returns>
        public virtual bool IsFolder() { return false; }
    }
}
