using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCloud.Users;
using System.Text.Json;

namespace NCloud.Models
{
    public class DbInitializer
    {
        private static CloudDbContext context = null!;
        private static UserManager<CloudUser> userManager = null!;
        private static SignInManager<CloudUser> signInManager = null!;
        private static readonly string JSONCONTAINERNAME = "__JsonContainer__.json";
        public static void Initialize(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            context = serviceProvider.GetRequiredService<CloudDbContext>();
            userManager = serviceProvider.GetRequiredService<UserManager<CloudUser>>();
            signInManager = serviceProvider.GetRequiredService<SignInManager<CloudUser>>();

            context.Database.Migrate();

            var admin = new CloudUser { FullName = "Admin", UserName = "Admin", Email="Admin@nclouddrive.hu" }; // Admin User : FullName: Admin, UserName: Admin

            if (!context.Users.Any())
            {
                try
                {
                    Task.Run(async () =>
                    {
                        var result = await userManager.CreateAsync(admin, "Admin_1234");
                        if (result.Succeeded)
                        {
                            string adminPath = Path.Combine(env.WebRootPath, "CloudData","UserData", admin.Id.ToString());
                            if (!Directory.Exists(adminPath))
                            {
                                Directory.CreateDirectory(adminPath);
                                CreateJsonConatinerFile(adminPath);
                                Directory.CreateDirectory(Path.Combine(adminPath,"Documents"));
                                CreateJsonConatinerFile(Path.Combine(adminPath, "Documents"));
                                Directory.CreateDirectory(Path.Combine(adminPath,"Pictures"));
                                CreateJsonConatinerFile(Path.Combine(adminPath, "Pictures"));
                                Directory.CreateDirectory(Path.Combine(adminPath,"Videos"));
                                CreateJsonConatinerFile(Path.Combine(adminPath, "Videos"));
                                Directory.CreateDirectory(Path.Combine(adminPath,"Music"));
                                CreateJsonConatinerFile(Path.Combine(adminPath, "Music"));
                            }
                        }

                    }).Wait(); // Passwords is Admin_1234 beacuse of safety reasons
                }
                catch { }
            }
            if (!Directory.Exists(Path.Combine(env.WebRootPath,"CloudData","Public")))
            {
                Directory.CreateDirectory(Path.Combine(env.WebRootPath,"CloudData", "Public"));
            }
            if (!Directory.Exists(Path.Combine(env.WebRootPath,"CloudData", "UserData")))
            {
                Directory.CreateDirectory(Path.Combine(env.WebRootPath,"CloudData", "UserData"));
            }
            context.SaveChanges();
        }

        private static void CreateJsonConatinerFile(string? path)
        {
            if (path is null) return;
            JsonDataContainer container = new JsonDataContainer()
            {
                FolderName = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Last()
            };
            System.IO.File.WriteAllText(Path.Combine(path, JSONCONTAINERNAME), JsonSerializer.Serialize<JsonDataContainer>(container));
        }
    }
}
