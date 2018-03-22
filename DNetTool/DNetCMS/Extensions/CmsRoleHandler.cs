using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace DNetCMS.Extensions
{
    public class CmsRoleHandler : AuthorizationHandler<CmsRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CmsRoleRequirement requirement)
        {
            

            return Task.CompletedTask;
        }
    }
}
