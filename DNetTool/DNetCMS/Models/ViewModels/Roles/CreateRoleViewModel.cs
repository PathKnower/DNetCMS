using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DNetCMS.Models.ViewModels.Roles
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Имя роли")]
        public string Name { get; set; }

        public SelectList Claims { get; set; }

        [Display(Name = "Уровень доступа")]
        public string SelectedAccessLevel { get; set; }
    }
}
