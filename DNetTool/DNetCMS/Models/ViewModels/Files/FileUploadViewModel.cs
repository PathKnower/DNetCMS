using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.ViewModels
{
    public class FileUploadViewModel
    {
        [Required]
        public IFormFile File { get; set; }

        [Display(Name = "Сохранить как: ")]
        public string TargetUse { get; set; }

        public string[] Targets { get; } = { "Изображение", "Документ", "Хранение" };
    }
}
