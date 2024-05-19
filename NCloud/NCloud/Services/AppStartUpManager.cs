using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.Users.Roles;
using System.Text.Json;

namespace NCloud.Services
{
    /// <summary>
    /// Statcic class to create resources for first startup
    /// </summary>
    public static class AppStartUpManager
    {
        private static CloudDbContext context = null!;
        private static UserManager<CloudUser> userManager = null!;
        private static SignInManager<CloudUser> signInManager = null!;
        private static RoleManager<CloudRole> roleManager = null!;
        private static ICloudService service = null!;
        private static ILogger logger = null!;

        /// <summary>
        /// Static method to initialize database, admin user and its base folder, log file folder, temp file folder
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="loggerService">Registered logger for logging</param>
        /// <exception cref="Exception"></exception>
        public static void Initialize(IServiceProvider serviceProvider, ILogger loggerService)
        {
            context = serviceProvider.GetRequiredService<CloudDbContext>();
            userManager = serviceProvider.GetRequiredService<UserManager<CloudUser>>();
            signInManager = serviceProvider.GetRequiredService<SignInManager<CloudUser>>();
            roleManager = serviceProvider.GetRequiredService<RoleManager<CloudRole>>();
            service = serviceProvider.GetRequiredService<ICloudService>();
            logger = loggerService;

            context.Database.Migrate();

            string adminRole = "admin";
            string userRole = "user";

            if (!roleManager.Roles.Any())
            {
                roleManager.CreateAsync(new CloudRole(adminRole, 10)).Wait();
                roleManager.CreateAsync(new CloudRole(userRole, 1)).Wait();
            }

            string pathHelper = Constants.GetPrivateBaseDirectory();

            try
            {
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
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Error while creating base directories: {ex.Message}");
            }

            if (!context.Users.Any())
            {
                var adminUser = new CloudUser { FullName = "Admin", UserName = "Admin", Email = "admin@nclouddrive.hu" };

                try
                {
                    userManager.CreateAsync(adminUser, "Admin_1234").Wait();  // Passwords is Admin_1234 because of safety reasons
                    userManager.AddToRoleAsync(adminUser, adminRole);

                }
                catch (Exception)
                {
                    logger.LogCritical("Exception while creating admin user");

                    return;
                }


                Task.Run(async () =>
                {
                    try
                    {
                        if (!await service.CreateBaseDirectoryForUser(adminUser))
                        {
                            throw new Exception("App unable to create base resources!");
                        }
                    }
                    catch (CloudLoggerException ex)
                    {
                        logger.LogCritical(ex.Message);
                    }
                    catch (Exception)
                    {
                        logger.LogCritical("Erro while creating base directory for admin");
                    }
                }).Wait();
            }

            context.SaveChanges();
        }
    }
}
