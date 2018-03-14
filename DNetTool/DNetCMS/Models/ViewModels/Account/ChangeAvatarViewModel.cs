using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace DNetCMS.Models.ViewModels.Account
{
    public class ChangeAvatarViewModel
    {
        public IFormFile NewAvatar { get; set; }
    }
}
