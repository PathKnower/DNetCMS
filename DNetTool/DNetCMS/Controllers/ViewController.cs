using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels.View;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class ViewController : Controller
    {
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        

        public ViewController(ApplicationContext context, IConfiguration configuration, IHostingEnvironment environment)
        {
            db = context;
            _configuration = configuration;
            _environment = environment;
        }

        public IActionResult Index()
        {
            bool.TryParse(_configuration.GetSection("Views")["OverrideBaseViews"], out bool canOverride);
            if(canOverride)
                return View(db.ViewOverrides.ToArray());
            else
            {
                HttpContext.Items["NoOverride"] = "К сожалению, невозможно изменить базовые представления. В найстройках программы, нет разрешения на это.";
                //TODO: Create change configuration action (Admin controller)
                return View();
            }
        }

        public IActionResult Create()
        {
            var model = new CreateReplaceViewModel {Views = GetNotOverrideViews().ToArray()};

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(CreateReplaceViewModel model)
        {
            if(!ModelState.IsValid)
            {
                model.Views = GetNotOverrideViews().ToArray();
                return View(model);
            }

            IEnumerable<string> temp = GetNotOverrideViews();
            if (!temp.Contains(model.ChoosenView))
            {
                ModelState.AddModelError("ChoosenView", "Нельяза переписать данное представление.");
                model.Views = temp.ToArray();
                return View(model);
            }

            var viewsPath = _configuration.GetSection("Views")["NewBaseViewsPath"];
            viewsPath = viewsPath[0] == '/' ? viewsPath : viewsPath.Insert(0, "/");
            viewsPath = viewsPath.Last() == '/' ? viewsPath : viewsPath.Append('/').ToString();

            if (Directory.Exists(_environment.WebRootPath + viewsPath))
                Directory.CreateDirectory(_environment.WebRootPath + viewsPath);

            FileInfo fileInfo = new FileInfo($"{_environment.WebRootPath}{viewsPath}{model.ChoosenView}.cshtml");
            if(fileInfo.Exists)
            {
                HttpContext.Items["Error"] = "Файл перезаписи для данного представления уже существует.";
                return RedirectToAction("Index");
            }

            try
            {
                StreamWriter sw = fileInfo.CreateText();
                sw.Write(model.Code);
                sw.Close();
            }
            catch (Exception)
            {
                HttpContext.Items["Error"] = "Не удалось записать в новый файл ваш код. Проверьте ваши права в системе.";
                return RedirectToAction("Index");
            }

            BaseViewOverride viewOverride = new BaseViewOverride
            {
                View = model.ChoosenView,
                Path = fileInfo.FullName,
                Enable = model.Enable
            };

            db.ViewOverrides.Add(viewOverride);
            db.SaveChanges();

            if(model.Enable)
                HttpContext.Items["Success"] = $"Перезапись представления \"{model.ChoosenView}\" успешно создана и активирована!";
            else
                HttpContext.Items["Success"] = $"Перезапись представления \"{model.ChoosenView}\" успешно создана! Вы можете активировать её в любой момент.";

            return RedirectToAction("Index");
        }

        private IEnumerable<string> GetNotOverrideViews()
        {
            string[] result = new string[]
            {
                //TODO: Fill that after end base front end
            };

            string[] overrideViews = db.ViewOverrides.Select(x => x.View).ToArray();

            return result.Except(overrideViews);
        }

        

        //TODO: what to do....
    }
}