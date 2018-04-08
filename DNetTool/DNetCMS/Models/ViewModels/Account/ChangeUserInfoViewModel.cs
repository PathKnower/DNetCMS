using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace DNetCMS.Models.ViewModels.Account
{
    public class ChangeUserInfoViewModel
    {
        [Display(Name = "ФИО")]
        public string FullName { get; set; }
        
        [Display(Name = "Дата рождения")]
        public DateTime DateOfBirth { get; set; }
        
        [Display(Name = "Новое фото профиля")]
        public IFormFile Avatar { get; set; }
    }
}
