using DNTCaptcha.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Security;
using NCloud.Services;
using NCloud.Users;
using NCloud.Users.Roles;
using System.Threading;

namespace NCloud
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<CloudDbContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection"));
                options.UseLazyLoadingProxies();
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<CloudUser, CloudRole>(options =>
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

            builder.Services.AddTransient<ICloudTerminalService, CloudTerminalService>();

            builder.Services.AddTransient<ICloudNotificationService, CloudNotificationService>();
          
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAntiforgery(x => x.HeaderName = "X-CSRF-TOKEN");

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddDNTCaptcha(options =>
                     options.UseCookieStorageProvider()
                            .ShowThousandsSeparators(false)
                            .AbsoluteExpiration(minutes: 7)
                            .RateLimiterPermitLimit(10)
                            .WithNoise(0.015f, 0.015f, 1, 0.0f)
                            .WithEncryptionKey(builder.Configuration.GetSection("EncryptionKey").Value)
            );

            builder.Logging.AddFile(Constants.GetLogFilePath());

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
                pattern: "{controller=Home}/{action=TestLogin}/{id?}");

            using (var serviceScope = app.Services.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<CloudDbContext>())
            {
                AppStartUpManager.Initialize(serviceScope.ServiceProvider);
            }

            Timer timer = new Timer(_ => new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CloudDirectoryManager.RemoveOutdatedItems();

            }).Start(), null, TimeSpan.FromSeconds(0), Constants.TempFileDeleteTimeSpan);

            app.Run();
        }
    }
}