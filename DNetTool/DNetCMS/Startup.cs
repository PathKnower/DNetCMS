using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using DNetCMS.Models.DataContract;
using Microsoft.AspNetCore.Mvc.Razor;
using DNetCMS.Extensions;
using DNetCMS.Interfaces;

namespace DNetCMS
{
    public class Startup
    {
        IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            //Base configure for start application
            Configuration = configuration;

            _env = environment;

            var builder = new ConfigurationBuilder().SetBasePath(_env.ContentRootPath);
            builder.AddJsonFile("DNetSettings.json", false, true);

            CmsConfiguration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IConfiguration CmsConfiguration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //string connection = "User ID=postgres;Password=dNetTool;Host=localhost;Port=5432;Database=DNetTool;Pooling=true;";

            services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(CmsConfiguration.GetSection("Database")["ConnectionString"]));

            services.AddIdentity<User, IdentityRole>(options => 
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<ApplicationContext>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminAccess", policy => policy.RequireClaim("AccessLevel", "Администратор"));
                options.AddPolicy("ModeratorAccess", policy => policy.RequireClaim("AccessLevel", "Администратор", "Модератор"));
                options.AddPolicy("WriterAccess", policy => policy.RequireClaim("AccessLevel", "Администратор", "Модератор", "Редактор"));
            });

            
            services.AddSingleton(provider => CmsConfiguration); //Добавление конфигурации в зависимости
            services.AddSingleton<ICacheStore, CacheStore>(); //Сервис хранения кэшированных данных 

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CMSViewLocator(CmsConfiguration));
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            logger.AddConsole();
            //logger.AddNLog();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });            
        }

        //public void SettingsFileCheck(string json)
        //{

        //}
    }
}
