using Microsoft.AspNetCore.Identity;
using NCloud.Models;
using System.ComponentModel.DataAnnotations;

namespace NCloud.Users
{
    public class CloudUser : IdentityUser
    {
        public string? FullName { get; set; }
        public virtual ICollection<SharedFolder> SharedFolders { get; set; }
        public virtual ICollection<SharedFile> SharedFiles { get; set; }

        public CloudUser()
        {
            SharedFolders = new List<SharedFolder>();
            SharedFiles = new List<SharedFile>();
        }
    }
}
