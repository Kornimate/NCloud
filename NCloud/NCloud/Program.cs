using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;

namespace NCloud
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<CloudDbContext>(options =>
            {
                IConfigurationRoot configuration = builder.Configuration;
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
                options.UseLazyLoadingProxies();
            });

            builder.Services.AddIdentity<CloudUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<CloudDbContext>();

            builder.Services.AddTransient<ICloudService, CloudService>();
            
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Drive}/{action=Index}/{id?}");

            using (var serviceScope = app.Services.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<CloudDbContext>())
            {
                DbInitializer.Initialize(serviceScope.ServiceProvider);
            }

            app.Run();
        }
    }
}