using Microsoft.AspNetCore.Identity;
using NCloud.Models;
using System.ComponentModel.DataAnnotations;

namespace NCloud.Users
{
    public class CloudUser : IdentityUser
    {
        public string? FullName { get; set; }
        public virtual ICollection<SharedFolder> InnerSharedFolders { get; set; }
        public virtual ICollection<SharedFolder> PublicSharedFolders { get; set; }
        public virtual ICollection<SharedFile> InnerSharedFiles { get; set; }
        public virtual ICollection<SharedFile> PublicSharedFiles { get; set; }

        public CloudUser()
        {
            InnerSharedFiles = new List<SharedFile>();
            PublicSharedFiles = new List<SharedFile>();
            InnerSharedFolders = new List<SharedFolder>();
            PublicSharedFolders = new List<SharedFolder>();
        }
    }
}
