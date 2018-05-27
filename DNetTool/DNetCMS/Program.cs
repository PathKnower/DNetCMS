using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DNetCMS.Models.DataContract;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Fluent;
using NLog.Web;
using NLog.Web.AspNetCore;


namespace DNetCMS
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        public static void Main(string[] args)
        {
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("Configurations/nlog.config", true);

            //var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            var logger = LogManager.GetCurrentClassLogger();
            
            try
            {
                logger.Info("Standing by...");
                var webHost = BuildWebHost(args);
                
                using (var scope = webHost.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetService<ApplicationContext>();
                    var userManager = services.GetService<UserManager<User>>();
                    var roleManager = services.GetService<RoleManager<IdentityRole>>();
                    SeedAdmin(context, userManager, roleManager);
                }
                
                logger.Info("Starting application");
                webHost.Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                LogManager.Shutdown();
            } 
            
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //.UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .UseConfiguration(Configuration)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog()
                .Build();

        public static void SeedAdmin(ApplicationContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var claim = context.UserClaims.FirstOrDefault(x =>
                x.ClaimType == "AccessLevel" && x.ClaimValue == "Администратор");
            if (claim == null)
            {
                User admin = new User
                {
                    UserName = "wardef",
                    Email = "admin@dnetcms.ru"    
                };
                userManager.CreateAsync(admin, "father");
                
                var role = new IdentityRole("Администратор");
                roleManager.CreateAsync(role);
                roleManager.AddClaimAsync(role, new Claim("AccessLevel", "Администратор"));

                userManager.AddToRoleAsync(admin, "Администратор");
            }
            
        }
        


    }
}
