using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DNetCMS.Models.ViewModels.Roles
{
    public class ChangeRoleClaims
    {
        public string Id { get; set; }

        [Display(Name = "Имя роли")]
        public string Name { get; set; }

        public SelectList Claims { get; set; }
        
        [Display(Name = "Уровень доступа")]
        public string SelectedAccessLevel { get; set; }
    }
}
