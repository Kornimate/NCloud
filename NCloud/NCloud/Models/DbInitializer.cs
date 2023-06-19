using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NCloud.Models
{
	public class DbInitializer
	{
		private static CloudDbContext context = null!;
		//private static UserManager<ApplicationUser> userManager = null!;
		public static void Initialize(IServiceProvider serviceProvider)
		{
			context = serviceProvider.GetRequiredService<CloudDbContext>();
			//userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			context.Database.Migrate();

			//if (!context.Users.Any())
			//{
			//	try
			//	{
			//		var user = new ApplicationUser { FullName = "Admin", UserName = "Admin" };
			//		userManager.CreateAsync(user,"Admin").Wait();
			//	}
			//	catch { }
			//}

			if(context.Entries.Any()) { return; }
		}
	}
}
