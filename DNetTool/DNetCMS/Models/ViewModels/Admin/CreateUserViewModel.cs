using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Models.ViewModels
{
    public class CreateUserViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
    }
}
