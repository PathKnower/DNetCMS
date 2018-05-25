using System;
using System.IO;
using System.Linq;
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
                    SeedAdmin(context);
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

        public static void SeedAdmin(ApplicationContext context)
        {
            var claim = context.RoleClaims.FirstOrDefault(x =>
                x.ClaimType == "AccessLevel" && x.ClaimValue == "Администратор");
            if (claim == null)
            {
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                
                User admin = new User
                {
                    UserName = "wardef",
                    Email = "admin@dnetcms.ru"    
                };

                admin.PasswordHash = hasher.HashPassword(admin, "father");
                var role = new IdentityRole("Администратор");

                 
                context.Roles.Add(role);
                context.Users.Add(admin);
                context.SaveChanges();

                var userRole = new IdentityUserRole<string>();
                userRole.RoleId = role.Id;
                userRole.UserId = admin.Id;
                
                claim = new IdentityRoleClaim<string>()
                {
                    ClaimType = "AccessLevel",
                    ClaimValue = "Администратор",
                    RoleId = role.Id
                };

                context.UserRoles.Add(userRole);
                context.RoleClaims.Add(claim);
                context.SaveChanges();
            }
            
        }
        
        //public static IWebHostBuilder CreateDefaultBuilder(string[] args)
        //{
        //    var builder = new WebHostBuilder()
        //        .UseKestrel((builderContext, options) =>
        //        {
        //            options.Configure(builderContext.Configuration.GetSection("Kestrel"));
        //        })
        //        .UseContentRoot(Directory.GetCurrentDirectory())
        //        .ConfigureAppConfiguration((hostingContext, config) =>
        //        {
        //            var env = hostingContext.HostingEnvironment;

        //            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        //            if (env.IsDevelopment())
        //            {
        //                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
        //                if (appAssembly != null)
        //                {
        //                    config.AddUserSecrets(appAssembly, optional: true);
        //                }
        //            }

        //            config.AddEnvironmentVariables();

        //            if (args != null)
        //            {
        //                config.AddCommandLine(args);
        //            }
        //        })
        //        .ConfigureLogging((hostingContext, logging) =>
        //        {
        //            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        //            logging.AddConsole();
        //            logging.AddDebug();
        //        })
        //        .UseIISIntegration()
        //        .UseDefaultServiceProvider((context, options) =>
        //        {
        //            options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
        //        });

        //    if (args != null)
        //    {
        //        builder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
        //    }

        //    return builder;
        //}


    }
}
