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
    public class CmsRoleRequirement : IAuthorizationRequirement
    {
        protected internal string[] Roles { get; set; }

        public CmsRoleRequirement(string[] roles)
        {
            Roles = roles;
        }
    }
}
