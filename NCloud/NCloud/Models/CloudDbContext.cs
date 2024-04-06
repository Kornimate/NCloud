using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using NCloud.Users.Roles;

namespace NCloud.Models
{
    public class CloudDbContext : IdentityDbContext<CloudUser,CloudRole,Guid>
    {
        public virtual DbSet<SharedFolder> SharedFolders { get; set; } = null!;
        public virtual DbSet<SharedFile> SharedFiles { get; set; } = null!;
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options) { }
    }
}
