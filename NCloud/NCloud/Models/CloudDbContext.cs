using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;

namespace NCloud.Models
{
    public class CloudDbContext : IdentityDbContext<CloudUser>
    {
        public virtual DbSet<SharedFolder> SharedFolders { get; set; } = null!;
        public virtual DbSet<SharedFile> SharedFiles { get; set; } = null!;
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options) { }
    }
}
