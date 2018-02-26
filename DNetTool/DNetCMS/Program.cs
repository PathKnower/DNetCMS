using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DNetCMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var config = new ConfigurationBuilder().AddCommandLine(args).Build();

            //var host = new WebHostBuilder()
            //    .UseConfiguration(config)
            //    .UseKestrel()
            //    .UseStartup<Startup>()
            //    .UseUrls("http://localhost:5000", "http://localhost:5001")
            //    .Build();

            //host.Run();
            var webHost = BuildWebHost(args);


            webHost.Run();
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

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
