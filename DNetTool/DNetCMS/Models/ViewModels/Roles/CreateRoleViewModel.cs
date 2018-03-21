using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DNetCMS.Models.ViewModels.Roles
{
    public class CreateRoleViewModel
    {
        [Required]
        public string Name { get; set; }

        public string[] Claims { get; set; }

        public string SelectedAccessLevel { get; set; }
    }
}
