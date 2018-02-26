using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DNetCMS.Models.DataContract
{
    public class Role : IdentityRole
    {
        public Role() : base() { }

        public Role(string name) : base(name) { }

        public Role ParentRole { get; set; }

    }
}
