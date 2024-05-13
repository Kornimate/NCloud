using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Users;
using NCloud.Users.Roles;
using System.Text.Json;

namespace NCloud.Services
{
    public static class AppStartUpManager
    {
        private static CloudDbContext context = null!;
        private static UserManager<CloudUser> userManager = null!;
        private static SignInManager<CloudUser> signInManager = null!;
        private static RoleManager<CloudRole> roleManager = null!;
        private static ICloudService service = null!;
        public static void Initialize(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            context = serviceProvider.GetRequiredService<CloudDbContext>();
            userManager = serviceProvider.GetRequiredService<UserManager<CloudUser>>();
            signInManager = serviceProvider.GetRequiredService<SignInManager<CloudUser>>();
            roleManager = serviceProvider.GetRequiredService<RoleManager<CloudRole>>();
            service = serviceProvider.GetRequiredService<ICloudService>();

            context.Database.Migrate();

            string adminRole = "admin";
            string userRole = "user";

            if (!roleManager.Roles.Any())
            {
                roleManager.CreateAsync(new CloudRole(adminRole, 10)).Wait();
                roleManager.CreateAsync(new CloudRole(userRole, 1)).Wait();
            }

            string pathHelper = Constants.GetPrivateBaseDirectory();

            if (!Directory.Exists(pathHelper))
            {
                Directory.CreateDirectory(pathHelper);
            }

            pathHelper = Constants.GetTempFileDirectory();

            if (!Directory.Exists(pathHelper))
            {
                Directory.CreateDirectory(pathHelper);
            }

            pathHelper = Constants.GetLogFilesDirectory();

            if (!Directory.Exists(pathHelper))
            {
                Directory.CreateDirectory(pathHelper);
            }

            if (!context.Users.Any())
            {
                var adminUser = new CloudUser { FullName = "Admin", UserName = "Admin", Email = "admin@nclouddrive.hu" };

                try
                {
                    userManager.CreateAsync(adminUser, "Admin_1234").Wait();  // Passwords is Admin_1234 because of safety reasons
                    userManager.AddToRoleAsync(adminUser, adminRole);

                }
                catch (Exception) { }


                Task.Run(async () =>
                {
                    if (!await service.CreateBaseDirectoryForUser(adminUser))
                    {
                        throw new Exception("App unable to create base resources!");
                    }
                }).Wait();
            }

            //Just sure to be created
            context.SaveChanges();
        }
    }
}
