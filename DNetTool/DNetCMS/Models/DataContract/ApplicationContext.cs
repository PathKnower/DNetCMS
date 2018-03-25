using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DNetCMS.Models.DataContract
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        private bool _viewUpdatePreapred;

        public delegate void CacheUpdatePrepare();
        public event CacheUpdatePrepare BaseViewOverrideUpdate;

        public DbSet<News> News { get; set; }

        public DbSet<FileModel> Files { get; set; }

        public DbSet<StaticView> StaticViews { get; set; }

        public DbSet<BaseViewOverride> ViewOverrides { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
           : base(options)
        {
            
        }

        public override EntityEntry<TEntity> Update<TEntity>([NotNull] TEntity entity)
        {
            if(entity is BaseViewOverride)
                _viewUpdatePreapred = true;

            return base.Update(entity);
        }

        public override int SaveChanges()
        {
            int result = base.SaveChanges();

            if (_viewUpdatePreapred)
                BaseViewOverrideUpdate();

            return result;
        }
    }
}
