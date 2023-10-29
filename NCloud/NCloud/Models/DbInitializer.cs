using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCloud.Services;
using NCloud.Users;
using System.Text.Json;

namespace NCloud.Models
{
    public class DbInitializer
    {
        private static CloudDbContext context = null!;
        private static UserManager<CloudUser> userManager = null!;
        private static SignInManager<CloudUser> signInManager = null!;
        private static ICloudService service = null!;
        public static void Initialize(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            context = serviceProvider.GetRequiredService<CloudDbContext>();
            userManager = serviceProvider.GetRequiredService<UserManager<CloudUser>>();
            signInManager = serviceProvider.GetRequiredService<SignInManager<CloudUser>>();
            service = serviceProvider.GetRequiredService<ICloudService>();

            context.Database.Migrate();


            if (!context.Users.Any())
            {
                try
                {
                    var admin = new CloudUser { FullName = "Admin", UserName = "Admin", Email = "Admin@nclouddrive.hu" };
                    userManager.CreateAsync(admin, "Admin_1234").Wait();  // Passwords is Admin_1234 beacuse of safety reasons
                    service.CreateBaseDirectory(admin);
                }
                catch { }
            }
            if (!Directory.Exists(Path.Combine(env.WebRootPath, "CloudData", "Public")))
            {
                Directory.CreateDirectory(Path.Combine(env.WebRootPath, "CloudData", "Public"));
            }
            if (!Directory.Exists(Path.Combine(env.WebRootPath, "CloudData", "UserData")))
            {
                Directory.CreateDirectory(Path.Combine(env.WebRootPath, "CloudData", "UserData"));
            }
            //Just sure to be created
            context.SaveChanges();
        }
    }
}
