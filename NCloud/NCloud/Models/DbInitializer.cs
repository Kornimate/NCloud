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
		public static void Initialize(IServiceProvider serviceProvider)
		{
			context = serviceProvider.GetRequiredService<CloudDbContext>();
			userManager = serviceProvider.GetRequiredService<UserManager<CloudUser>>();

			context.Database.Migrate();

			var admin = new CloudUser { FullName = "Admin", UserName = "Admin" }; // Admin User : FullName: Admin, UserName: Admin

			if (!context.Users.Any())
			{
				try
				{
					userManager.CreateAsync(admin, "Admin_1234").Wait(); // Passwords is Admin_1234 beacuse of safety reasons
				}
				catch { }
			}

			if (context.Entries.Any()) { return; }

			List<Entry> defFolders = new List<Entry>()
			{
				new Entry
				{
					Name="Public Folder",
					Size=0,
					ParentId=0,
					Type = EntryType.FOLDER,
					CreatedDate= DateTime.Now,
					IsVisibleForEveryOne = true,
					CreatedBy = admin,
				}
			};

			context.Entries.AddRange(defFolders);
			context.SaveChanges();
		}
	}
}
