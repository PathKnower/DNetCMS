using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.ViewModels
{
    public class NewsViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Заголовок")]
        [MaxLength(65, ErrorMessage = "Заголовок не должен первышать 65 символов.")]
        [MinLength(3, ErrorMessage = "Заголовок не может быть меньше 3 символов.")]
        public string Header { get; set; }

        [Required]
        [Display(Name = "Содержимое")]
        [MinLength(30, ErrorMessage = "Статья не может быть меньше 30 символов.")]
        public string Content { get; set; }
        
        public DateTime CreateDate { get; set; }

        [NotMapped]
        public IFormFile Picture { get; set; }
    }
}
