using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Attributes
{
    public class CmsAuthorizePolitic
    {

    }

    //public class CmsAuthorizeAttribute : AuthorizeAttribute
    //{
    //    public CmsAuthorizeAttribute(params string[] roleKeys)
    //    {
    //        var roles = new List<string>();
    //        var allRoles = (NameValueCollection)ConfigurationManager.GetSection("CustomRoles");
    //        foreach (var roleKey in roleKeys)
    //        {
    //            roles.AddRange(allRoles[roleKey].Split(new[] { ',' }));
    //        }

    //        Roles = string.Join(",", roles);
    //    }

    //    public override void OnAuthorize(AuthorizationContext filterContext)
    //    {
    //        base.OnAuthorization(filterContext);
    //        if (filterContext.Result is HttpUnauthorizedResult)
    //        {
    //            filterContext.Result = new RedirectResult("~/Error/AcessDenied");
    //        }
    //    }
    //}
}
