using System.Linq;
using DNetCMS.Interfaces;
using Microsoft.AspNetCore.Mvc;

using DNetCMS.Models.DataContract;

namespace DNetCMS.Extensions
{
    public abstract class CmsController : Controller
    {
        private ICacheStore _cacheStore;
        
        public CmsController(ICacheStore cacheStore)
        {
            _cacheStore = cacheStore;
        }
        
        [NonAction]
        public override ViewResult View()
        {
            var overridenView = _cacheStore.ViewOverrides.
                FirstOrDefault(x => x.View == $"{ControllerContext.ActionDescriptor.ControllerName}/{ControllerContext.ActionDescriptor.ActionName}");

            if (overridenView != null && overridenView.Enable)
                return base.View(overridenView.Path);
            
            return base.View();
        }

        [NonAction]
        public override ViewResult View(object model)
        {    
            var overridenView = _cacheStore.ViewOverrides.
                FirstOrDefault(x => x.View == $"{ControllerContext.ActionDescriptor.ControllerName}/{ControllerContext.ActionDescriptor.ActionName}");

            if (overridenView != null && overridenView.Enable)
                return base.View(overridenView.Path, model);
            
            return base.View(model);
        }
        
        
    }
}