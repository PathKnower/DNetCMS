using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.DataContract
{
    public class User
    {

        [Key]
        public int Id { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        //[Display(Name = "Имя", Description = "Укажите ваше Имя, чтобы знать, как к вам обращаться")]
        public string FirstName { get; set; }

        //[Display(Name = "Фамилия", Description = "Укажите вашу Фамилию, чтобы знать, как к вам обращаться")]
        public string LastName { get; set; }

        public string Password { get; set; }

        public DateTime CreateDate { get; set; }

         
        //Для своего identity
        public int? RoleId { get; set; }

        public Role UserRole { get; set; }
    }
}
