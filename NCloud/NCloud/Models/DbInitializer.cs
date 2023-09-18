using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCloud.Users;

namespace NCloud.Models
{
    public class DbInitializer
    {
        private static CloudDbContext context = null!;
        private static UserManager<CloudUser> userManager = null!;
        private static SignInManager<CloudUser> signInManager = null!;
        public static void Initialize(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            context = serviceProvider.GetRequiredService<CloudDbContext>();
            userManager = serviceProvider.GetRequiredService<UserManager<CloudUser>>();
            signInManager = serviceProvider.GetRequiredService<SignInManager<CloudUser>>();

            context.Database.Migrate();

            var admin = new CloudUser { FullName = "Admin", UserName = "Admin" }; // Admin User : FullName: Admin, UserName: Admin

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
                                Directory.CreateDirectory(Path.Combine(adminPath,"Documents"));
                                Directory.CreateDirectory(Path.Combine(adminPath,"Pictures"));
                                Directory.CreateDirectory(Path.Combine(adminPath,"Videos"));
                                Directory.CreateDirectory(Path.Combine(adminPath,"Music"));
                            }
                        }

                    }).Wait(); // Passwords is Admin_1234 beacuse of safety reasons
                }
                catch { }
                //if (context.Entries.Any()) { return; }

                //List<Entry> defFolders = new List<Entry>()
                //{
                //	new Entry
                //	{
                //		Name="Public Folder",
                //		Size=0,
                //		ParentId=0,
                //		Type = EntryType.FOLDER,
                //		CreatedDate= DateTime.Now,
                //		IsVisibleForEveryOne = true,
                //		CreatedBy = admin,
                //	}
                //};

                //context.Entries.AddRange(defFolders);
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
    }
}
