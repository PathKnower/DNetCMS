using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DNetCMS.Interfaces;
using DNetCMS.Models.DataContract;

namespace DNetCMS.Extensions
{
    public class CacheStore : ICacheStore
    {
        public IList<BaseViewOverride> ViewOverrides { get; private set; }

        private readonly ApplicationContext db;

        public CacheStore(ApplicationContext context)
        {
            db = context;
            db.BaseViewOverrideUpdate += Db_BaseViewOverrideUpdate;
            Db_BaseViewOverrideUpdate();
        }

        private void Db_BaseViewOverrideUpdate()
        {
            ViewOverrides = db.ViewOverrides.ToList();
        }
    }
}
