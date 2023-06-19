using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;

namespace NCloud.Models
{
    public class CloudDbContext : IdentityDbContext<CloudUser>
    {
        public DbSet<Entry> Entries { get; set; } = null!;
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options) { }
    }
}
