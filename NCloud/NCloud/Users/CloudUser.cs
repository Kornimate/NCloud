using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NCloud.Users
{
    public class CloudUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
