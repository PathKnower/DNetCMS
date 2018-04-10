using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.ViewModels.Roles
{
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Имя роли")]
        public string Name { get; set; }

        [Display(Name = "Уровень доступа")]
        public string AccessLevel { get; set; }
    }
}
