﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using NCloud.Users.Roles;

namespace NCloud.Models
{
    /// <summary>
    /// DbContext for Entity-Framework to handle database connection
    /// </summary>
    public class CloudDbContext : IdentityDbContext<CloudUser, CloudRole, Guid>
    {
        public virtual DbSet<SharedFolder> SharedFolders { get; set; } = null!;
        public virtual DbSet<SharedFile> SharedFiles { get; set; } = null!;
        public virtual DbSet<CloudSpaceRequest> CloudSpaceRequests { get; set; } = null!;
        public virtual DbSet<CloudLogin> Logins { get; set; } = null!;
        public CloudDbContext(DbContextOptions<CloudDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CloudUser>().HasIndex(x => x.UserName).IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
