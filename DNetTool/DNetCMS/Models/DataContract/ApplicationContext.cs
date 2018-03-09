using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DNetCMS.Models.DataContract
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<News> News { get; set; }

        public DbSet<FileModel> Files { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
           : base(options)
        {
            
        }
    }
}
