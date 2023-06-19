using Microsoft.EntityFrameworkCore;

namespace NCloud.Models
{
    public class CloudDbContext : DbContext
    {
        public DbSet<Entry> Entries { get; set; } = null!;
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options) { }
    }
}
