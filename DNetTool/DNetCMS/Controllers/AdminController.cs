using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using DNetCMS.Models.DataContract;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class AdminController : Controller
    {
        private readonly ApplicationContext db;
        private readonly UserManager<User> _userManager;
        private IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            ILogger<AdminController> logger
            )
        {
            db = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }


        public IActionResult Index()
        {
//            HttpContext.Items["ErrorMessage"] = "Some error message";
//            HttpContext.Items["WarningMessage"] = "Some warning message";
//            HttpContext.Items["SuccessMessage"] = "Some success message";
            
            return View();
        }



    }
}