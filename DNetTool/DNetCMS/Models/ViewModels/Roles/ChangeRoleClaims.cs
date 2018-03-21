using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.ViewModels.Roles
{
    public class ChangeRoleClaims
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string[] Claims { get; set; }

        public string SelectedAccessLevel { get; set; }
    }
}
