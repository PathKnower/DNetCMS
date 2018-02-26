using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNetCMS.Models.DataContract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DNetCMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly RoleManager<Role> _roleManaegr;

        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ILogger<AdminController> logger
            )
        {
            _userManager = userManager;
            _roleManaegr = roleManager;
            _logger = logger;
        }


        public IActionResult Index()
        {
            return View();
        }



    }
}