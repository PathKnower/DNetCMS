using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using DNetCMS.Extensions;
using DNetCMS.Interfaces;
using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels;

namespace DNetCMS.Controllers
{
    public class HomeController : CmsController
    {
        ApplicationContext db;
        private ICacheStore _cacheStore;

        public HomeController(ICacheStore cacheStore) : base(cacheStore)
        {
            _cacheStore = cacheStore;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
