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
using DNetCMS.Middleware;
using DNetCMS.Modules.Processing;
using Microsoft.AspNetCore.DataProtection;

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
            builder.AddJsonFile("DNetSettings.json", optional: false, reloadOnChange: true);

            CmsConfiguration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IConfiguration CmsConfiguration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: не забыть сменить среду на продакшн
            services.AddDbContext<ApplicationContext>(options => 
                options.UseNpgsql(CmsConfiguration.GetSection("Database")["ConnectionString"]), ServiceLifetime.Singleton);
            
            services.AddIdentity<User, IdentityRole>(options => 
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
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
            services.AddScoped<FileProcessing>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger)
        {
            app.UseMiddleware(typeof(ExceptionHandler));
            
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
        //TODO Не забыть чекать конфиг файл
        //}
    }
}
