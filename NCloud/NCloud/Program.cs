using DNTCaptcha.Core;
using Microsoft.EntityFrameworkCore;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Security;
using NCloud.Services;
using NCloud.Users;
using NCloud.Users.Roles;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

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
                //Password
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;

                //User data
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<CloudDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                //Lockkout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                //Sign in
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = Constants.AppCookieName;
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/UserManagement/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            builder.Services.Configure<PasswordHasherOptions>(option =>
            {
                option.IterationCount = 12000;
            });

            builder.Services.Configure<SecurityStampValidatorOptions>(o =>
                   o.ValidationInterval = TimeSpan.FromMinutes(1));

            builder.Services.AddTransient<ICloudService, CloudService>();

            builder.Services.AddTransient<ICloudTerminalService, CloudTerminalService>();

            builder.Services.AddTransient<ICloudNotificationService, CloudNotificationService>();

            builder.Services.AddTransient<IEmailSender, CloudEmailService>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();

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
                            .WithEncryptionKey(builder.Configuration.GetSection("EncryptionKey").Value!)
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

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var serviceScope = app.Services.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<CloudDbContext>())
            {
                CloudStartUpManager.Initialize(serviceScope.ServiceProvider, app.Logger);
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