﻿using Microsoft.AspNetCore.Identity;

namespace NCloud.Users.Roles
{
    public class CloudRole : IdentityRole<Guid>
    {
        public int Level { get; set; }
        public CloudRole(string name, int level) : base(name)
        {
            Level = level;
        }
    }
}