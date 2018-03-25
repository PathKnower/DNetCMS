using DNetCMS.Interfaces;
using DNetCMS.Models.DataContract;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Extensions
{
    public class CMSViewLocator : IViewLocationExpander
    {
        IConfiguration _cmsConfig;
        //ICacheStore _cacheStore;

        public CMSViewLocator(IConfiguration configuration)
        {
            _cmsConfig = configuration;
            //_cacheStore = cacheStore;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        { }

        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            try
            {
                //var overrideView = _cacheStore.ViewOverrides.FirstOrDefault(x => x.View == $"{context.ControllerName}/{context.ViewName}");
                //if(overrideView == null)
                //    return viewLocations;

                bool.TryParse(_cmsConfig.GetSection("Views")["OverrideBaseViews"], out bool canOverride);
                if(canOverride)
                {
                    string viewsPath = _cmsConfig.GetSection("Views").GetValue("NewBaseViewsPath", "/Views/");
                    return viewLocations.Select(f => f.Replace("/Views/", viewsPath));
                }
                else
                    return viewLocations;
            }
            catch(Exception)
            {
                return viewLocations;
            }
        }
    }
}
