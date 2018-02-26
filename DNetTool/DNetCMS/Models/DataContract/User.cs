using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DNetCMS.Models.DataContract
{
    public class User : IdentityUser
    {
        //[Display(Name = "Имя", Description = "Укажите ваше Имя, чтобы знать, как к вам обращаться")]
        public string FirstName { get; set; }

        //[Display(Name = "Фамилия", Description = "Укажите вашу Фамилию, чтобы знать, как к вам обращаться")]
        public string LastName { get; set; }

        public DateTime CreateDate { get; set; }

        public int Age { get; set; }
    }
}
