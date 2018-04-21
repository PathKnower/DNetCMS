using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DNetCMS.Models.ViewModels.View
{
    public class CreateReplaceViewModel
    {
        [Required]
        [Display(Name = "Выбранная страница для перезаписи")]
        public string ChoosenView { get; set; }

        public SelectList Views { get; set; }

        [Display(Name = "Активен")]
        public bool Enable { get; set; }

        [Required]
        [Display(Name = "Код страницы")]
        public string Code { get; set; }
    }
}
