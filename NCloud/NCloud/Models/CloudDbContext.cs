using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;

namespace NCloud.Models
{
    public class CloudDbContext : IdentityDbContext<CloudUser>
    {
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options) { }
    }
}
