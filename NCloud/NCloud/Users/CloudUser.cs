using Microsoft.AspNetCore.Identity;
using NCloud.ConstantData;
using NCloud.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCloud.Users
{
    /// <summary>
    /// Class to create users for database
    /// </summary>
    public class CloudUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public double UsedSpace { get; set; }
        public double MaxSpace { get; set; } = Constants.UserSpaceSize;
        public virtual ICollection<SharedFolder> SharedFolders { get; set; }
        public virtual ICollection<SharedFile> SharedFiles { get; set; }
        public virtual ICollection<CloudLogin> Logins { get; set; }
        public virtual CloudSpaceRequest? CloudSpaceRequest { get; set; }

        public CloudUser() : base()
        {
            SharedFolders = new List<SharedFolder>();
            SharedFiles = new List<SharedFile>();
            Logins = new List<CloudLogin>();
        }

        public CloudUser(string userName, string fullName) : base(userName)
        {
            FullName = fullName;
            UsedSpace = 0;
            MaxSpace = Constants.UserSpaceSize;
            SharedFolders = new List<SharedFolder>();
            SharedFiles = new List<SharedFile>();
            Logins = new List<CloudLogin>();
        }
    }
}
