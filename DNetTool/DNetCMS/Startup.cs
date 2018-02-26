﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using DNetCMS.Models.DataContract;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DNetCMS.Interfaces;
using DNetCMS.Repositories;
using Microsoft.Extensions.Logging;

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
            builder.AddJsonFile("DNetSettings.json");

            CmsConfiguration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IConfiguration CmsConfiguration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //string connection = "User ID=postgres;Password=dNetTool;Host=localhost;Port=5432;Database=DNetTool;Pooling=true;";

            services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(CmsConfiguration.GetSection("Database")["ConnectionString"]));

            services.AddIdentity<User, Role>().AddEntityFrameworkStores<ApplicationContext>();


            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddSingleton<IUnitOfWork, UnitOfWork>();

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
