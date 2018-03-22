using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using DNetCMS.Models.DataContract;
using Microsoft.EntityFrameworkCore;

namespace DNetCMS.Controllers
{
    public class GenericController : Controller
    {
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GenericController> _logger;

        public GenericController(ApplicationContext context,
            IConfiguration configuration,
            ILogger<GenericController> logger)
        {
            db = context;
            _configuration = configuration;
            _logger = logger;
        }

        [Route("generic/{*route}")]
        public async Task<IActionResult> GetView(string route)
        {
            if (route[0] == '/')
                route.Remove(0, 1);

            StaticView view = await db.StaticViews.FirstOrDefaultAsync(x => x.Route == route);
            if (view == null)
                return NotFound("Страница не найдена.");
            
            return View(viewName: view.Path);
        }
    }
}