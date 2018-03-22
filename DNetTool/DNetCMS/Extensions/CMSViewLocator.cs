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

        public CMSViewLocator(IConfiguration configuration)
        {
            _cmsConfig = configuration;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        { }

        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            try
            {
                string viewsPath = _cmsConfig.GetSection("Views").GetValue("BaseViewsPath", "/Views/");
                return viewLocations.Select(f => f.Replace("/Views/", viewsPath));
            }
            catch(Exception)
            {
                return viewLocations;
            }
        }
    }
}
