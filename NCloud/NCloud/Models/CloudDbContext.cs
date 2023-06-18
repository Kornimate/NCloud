using Microsoft.EntityFrameworkCore;

namespace NCloud.Models
{
    public class CloudDbContext : DbContext
    {
        public DbSet<File> Files { get; set; } = null!;
        public DbSet<Folder> Folders { get; set; } = null!;
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options)
        {

        }
    }
}
