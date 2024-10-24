using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.HostedServices;
using NCloud.Users;
using NCloud.Users.Roles;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NCloud.Security.HealthCheck;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;

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
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._@+";
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

            builder.Services.AddRateLimiter(_ => _
                .AddFixedWindowLimiter(policyName: "fixed", options =>
                {
                    options.PermitLimit = 5;
                    options.Window = TimeSpan.FromSeconds(10);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                }));

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 41943040; //40 MB in binary
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });

            builder.Services.AddTransient<ICloudService, CloudService>();

            builder.Services.AddTransient<ICloudTerminalService, CloudTerminalService>();

            builder.Services.AddTransient<ICloudNotificationService, CloudNotificationService>();

            builder.Services.AddTransient<IEmailSender, CloudEmailService>();

            builder.Services.AddHostedService<CloudDirectoryManagerHostedService>();

            builder.Services.AddHostedService<CloudDatabaseManagerHostedService>();

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

            builder.Services.AddHealthChecks()
                            .AddCheck<RemoteHealthCheck>("Endpoints Health Check", failureStatus: HealthStatus.Unhealthy, tags: new[] { "Endpoint" })
                            .AddCheck<MemoryHealthCheck>($"Memory Check", failureStatus: HealthStatus.Unhealthy, tags: new[] { "Memory" });

            builder.Services.AddHealthChecksUI(opt =>
            {
                opt.SetEvaluationTimeInSeconds(30); //secs between tests
                opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks    
                opt.SetApiMaxActiveRequests(2); //api requests concurrency    
                opt.AddHealthCheckEndpoint("Cloud Health Check API", "/api/cloud-health"); //map health check api    

            })
                .AddInMemoryStorage();

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

            app.MapHealthChecks("/api/cloud-health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(delegate (Options options)
            {
                options.UIPath = "/CloudHealthCheck";
            });

            using (var serviceScope = app.Services.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<CloudDbContext>())
            {
                CloudStartUpManager.Initialize(serviceScope.ServiceProvider, app.Logger);
            }

            app.Run();
        }
    }
}