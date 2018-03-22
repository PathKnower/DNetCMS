using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DNetCMS.Models.ViewModels.StaticViews
{
    public class CreateStaticViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Route { get; set; }
    }
}
