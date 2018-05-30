using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.ViewModels
{
    public class FileUploadViewModel
    {
        [Display(Name = "Файл для загрузки")]
        public IFormFile File { get; set; }

        [Display(Name = "Сохранить как: ")]
        public string TargetUse { get; set; }

        public SelectList Targets { get; set; } = new SelectList(new string[] { "Изображение", "Документ", "Хранение" });
    }
}
