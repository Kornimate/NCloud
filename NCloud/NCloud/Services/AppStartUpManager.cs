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
        private static IConfiguration config = null!;
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
            config = serviceProvider.GetRequiredService<IConfiguration>();
            logger = loggerService;

            context.Database.Migrate();

            if (!roleManager.Roles.Any(x => x.Name == Constants.AdminRole))
            {
                var adminRoleCreated = roleManager.CreateAsync(new CloudRole(Constants.AdminRole, 10)).GetAwaiter().GetResult();

                if (!adminRoleCreated.Succeeded)
                    logger.LogCritical("Admin role not created");
            }

            if (!roleManager.Roles.Any(x => x.Name == Constants.UserRole))
            {
                var userRoleCreated = roleManager.CreateAsync(new CloudRole(Constants.UserRole, 1)).GetAwaiter().GetResult();

                if (!userRoleCreated.Succeeded)
                    logger.LogCritical("User role not created");
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

            var adminUser = service.GetAdmin().GetAwaiter().GetResult();

            if (adminUser == null || !userManager.IsInRoleAsync(adminUser, Constants.AdminRole).GetAwaiter().GetResult())
            {
                adminUser = new CloudUser { FullName = Constants.AdminUserName, UserName = Constants.AdminUserName, Email = "admin@nclouddrive.hu" };

                try
                {
                    var adminCreated = userManager.CreateAsync(adminUser, config.GetSection("AdminPassword").Value).GetAwaiter().GetResult();  // Passwords is defined in appSettings.json
                    var adminAddedToRole = userManager.AddToRoleAsync(adminUser, Constants.AdminRole).GetAwaiter().GetResult();

                    if (!adminCreated.Succeeded)
                        logger.LogCritical("Admin user can not be created");

                    if (!adminAddedToRole.Succeeded)
                        logger.LogCritical("Admin not added to admin role");
                }
                catch (Exception)
                {
                    logger.LogCritical("Exception while creating admin user");

                    userManager.DeleteAsync(adminUser).Wait();

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
