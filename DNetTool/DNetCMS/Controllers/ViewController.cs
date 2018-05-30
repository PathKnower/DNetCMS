using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels.View;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class ViewController : Controller
    {
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly ILogger<ViewController> _logger;
        

        public ViewController(ApplicationContext context, 
            IConfiguration configuration, 
            IHostingEnvironment environment,
            ILogger<ViewController> logger)
        {
            db = context;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            bool.TryParse(_configuration.GetSection("Views")["OverrideBaseViews"], out bool canOverride);
            if(canOverride)
                return View(db.ViewOverrides.ToArray());
            
            HttpContext.Items["NoOverride"] = "К сожалению, невозможно изменить базовые представления. В найстройках программы, нет разрешения на это.";
            //TODO: Create change configuration action (Admin controller)
            return View();
        }

        public IActionResult Create()
        {
            var model = new CreateReplaceViewModel {Views = new SelectList(GetNotOverrideViews())};

            return View(model);
        }

        public IActionResult Edit(string view)
        {

            BaseViewOverride viewOverride = db.ViewOverrides.FirstOrDefault(x => x.View == view);
            if (viewOverride == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти переписанную вами страницу.";
                return RedirectToAction("Index");
            }

            EditBaseViewOverride model = new EditBaseViewOverride
            {
                ChoosenView = view,
                OldView = viewOverride.View,
                Enable = viewOverride.Enable,
                Code = ReadFromFile(viewOverride.Path),
                Views = new SelectList(GetNotOverrideViews())
            };
            
            return View(model);
        }

        public IActionResult Delete(string viewName)
        {
            BaseViewOverride viewOverride = db.ViewOverrides.FirstOrDefault(x => x.View == viewName);
            if (viewOverride == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти переписанную вами страницу.";
                return RedirectToAction("Index");
            }

            var model = new DeleteBaseViewOverride
            {
                ChoosenView = viewOverride.View,
                Enable = viewOverride.Enable
            };
            
            return View();
        }

        public IActionResult EnableView(string view, bool enable)
        {
            BaseViewOverride viewOverride = db.ViewOverrides.FirstOrDefault(x => x.View == view);
            if (viewOverride == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти переписанную вами страницу.";
                return RedirectToAction("Index");
            }
            
            _logger.LogDebug("Enable view action start for view = {@viewOverrid}", viewOverride);
            viewOverride.Enable = enable;

            db.ViewOverrides.Update(viewOverride);
            db.SaveChanges();
            
            _logger.LogInformation("View = {@view}, was switch enable state to {enable}", viewOverride, enable);
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Create(CreateReplaceViewModel model)
        {
            if(!ModelState.IsValid)
            {
                model.Views = new SelectList(GetNotOverrideViews());
                return View(model);
            }

            IEnumerable<string> temp = GetNotOverrideViews();
            if (!temp.Contains(model.ChoosenView))
            {
                ModelState.AddModelError("ChoosenView", "Нельяза переписать данное представление.");
                model.Views = new SelectList(temp);
                return View(model);
            }

            _logger.LogDebug("Create overridable view action started");
            var viewsPath = _configuration.GetSection("Views")["NewBaseViewsPath"];
            viewsPath = viewsPath[0] == '/' ? viewsPath : viewsPath.Insert(0, "/");
            viewsPath = viewsPath.Last() == '/' ? viewsPath : viewsPath.Append('/').ToString();

            _logger.LogDebug("Check directory existance");
            if (!Directory.Exists(_environment.ContentRootPath + viewsPath))
                Directory.CreateDirectory(_environment.ContentRootPath + viewsPath);

            var fileInfo = new FileInfo($"{_environment.ContentRootPath}{viewsPath}{model.ChoosenView}.cshtml");
            if(fileInfo.Exists)
            {
                HttpContext.Items["ErrorMessage"] = "Файл перезаписи для данного представления уже существует.";
                return RedirectToAction("Index");
            }

            if (!Directory.Exists($"{_environment.ContentRootPath + viewsPath}/{model.ChoosenView.Split('/')[0]}"))
                Directory.CreateDirectory($"{_environment.ContentRootPath + viewsPath}/{model.ChoosenView.Split('/')[0]}");

            _logger.LogDebug("Try to create file");
            try
            {
                StreamWriter sw = fileInfo.CreateText();
                sw.Write(model.Code);
                sw.Close();
            }
            catch (Exception)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось записать в новый файл ваш код. Проверьте ваши права в системе.";
                return RedirectToAction("Index");
            }
            _logger.LogDebug("File successfully created, create view entity model");

            var viewOverride = new BaseViewOverride
            {
                View = model.ChoosenView,
                Path = $"{viewsPath}{model.ChoosenView}.cshtml",
                Enable = model.Enable
            };

            db.ViewOverrides.Add(viewOverride);
            db.SaveChanges();
            _logger.LogInformation("New overridable page created! View = {@view}", viewOverride);
            
            if(model.Enable)
                HttpContext.Items["SuccessMessage"] = $"Перезапись представления \"{model.ChoosenView}\" успешно создана и активирована!";
            else
                HttpContext.Items["SuccessMessage"] = $"Перезапись представления \"{model.ChoosenView}\" успешно создана! Вы можете активировать её в любой момент.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditBaseViewOverride model)
        {
            if(!ModelState.IsValid)
            {
                model.Views = new SelectList(GetNotOverrideViews());
                return View(model);
            }

            //TODO: Fix read from file
            IEnumerable<string> temp = GetNotOverrideViews();
            if (!temp.Contains(model.ChoosenView))
            {
                ModelState.AddModelError("ChoosenView", "Нельяза переписать данное представление.");
                model.Views = new SelectList(temp);
                return View(model);
            }

            _logger.LogDebug("Edit overridable view action started");

            var viewOverride = db.ViewOverrides.FirstOrDefault(x => x.View == model.OldView);
            if (viewOverride == null)
            {
                _logger.LogDebug($"Cannot find view override entity with view = {model.OldView}");
                HttpContext.Items["ErrorMessage"] = "Не удалось найти изменяемую страницу";
                return RedirectToAction("Index");
            }
            
            var viewsPath = _configuration.GetSection("Views")["NewBaseViewsPath"];
            viewsPath = viewsPath[0] == '/' ? viewsPath : viewsPath.Insert(0, "/");
            viewsPath = viewsPath.Last() == '/' ? viewsPath : viewsPath.Append('/').ToString();

            _logger.LogDebug("Check directory existance");
            if (!Directory.Exists(_environment.ContentRootPath + viewsPath))
                Directory.CreateDirectory(_environment.ContentRootPath + viewsPath);

            
            var oldFile = new FileInfo(viewOverride.Path);
            var fileInfo = new FileInfo($"{_environment.ContentRootPath}{viewsPath}{model.ChoosenView}.cshtml");
            if(fileInfo.Exists)
            {
                HttpContext.Items["ErrorMessage"] = "Файл перезаписи для данного представления уже существует.";
                return RedirectToAction("Index");
            }

            if (!Directory.Exists($"{_environment.ContentRootPath + viewsPath}/{model.ChoosenView.Split('/')[0]}"))
                Directory.CreateDirectory($"{_environment.ContentRootPath + viewsPath}/{model.ChoosenView.Split('/')[0]}");

            _logger.LogDebug("Try to create file");
            try
            {
                StreamWriter sw = fileInfo.CreateText();
                sw.Write(model.Code);
                sw.Close();
            }
            catch (Exception)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось записать в новый файл ваш код. Проверьте ваши права в системе.";
                return RedirectToAction("Index");
            }
            _logger.LogDebug("File successfully created, create view entity model");

            _logger.LogDebug("Remove old file on path = " + oldFile.FullName);
            if(oldFile.Exists)
                oldFile.Delete();
            
            viewOverride.Enable = model.Enable;
            viewOverride.Path = $"{viewsPath}{model.ChoosenView}.cshtml";
            viewOverride.View = model.ChoosenView;

            db.ViewOverrides.Update(viewOverride);
            db.SaveChanges();
            _logger.LogInformation($"Overridable page edited! View = {viewOverride.View}");
            
            if(model.Enable)
                HttpContext.Items["SuccessMessage"] = $"Перезапись представления \"{model.ChoosenView}\" успешно создана и активирована!";
            else
                HttpContext.Items["SuccessMessage"] = $"Перезапись представления \"{model.ChoosenView}\" успешно создана! Вы можете активировать её в любой момент.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> Remove(string viewName)
        {
            BaseViewOverride viewOverride = db.ViewOverrides.FirstOrDefault(x => x.View == viewName);
            if (viewOverride == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти переписанную вами страницу";
                return RedirectToAction("Index");
            }
            
            _logger.LogDebug($"Remove overridable view with viewname = {viewOverride.View}");
            
            var fileInfo = new FileInfo(viewOverride.Path);
            
            _logger.LogDebug("Try to delete view file");
            
            if(fileInfo.Exists)
                fileInfo.Delete();
            
            _logger.LogDebug("Try to delete view from database");
            db.ViewOverrides.Remove(viewOverride);
            await db.SaveChangesAsync();
            
            _logger.LogDebug("Successfully remove overridable view");
            HttpContext.Items["SuccessMessage"] = "Удаление страницы произведено успешно";
            
            return RedirectToAction("Index");
        }
        

        
        
        private string[] GetNotOverrideViews()
        {
            string[] result = new string[]
            {
                "Home/Index",
                "Home/About",
                "News/Index"
                //TODO: Fill that after end base front end
            };

            string[] overrideViews = db.ViewOverrides.Select(x => x.View).ToArray();

            return result.Except(overrideViews).ToArray();
        }
        
        private string ReadFromFile(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string result = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                return result;
            }
            catch (Exception)
            {
                return "Ошибка чтения файла. Проверьте имеется ли доступ к файлам.";
            }
        }

    }
}