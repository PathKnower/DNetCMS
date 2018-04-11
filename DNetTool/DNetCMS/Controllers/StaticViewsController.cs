﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels.StaticViews;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class StaticViewsController : Controller
    {
        ApplicationContext db;
        IHostingEnvironment _environment;
        IConfiguration _configuration;
        //ILogger<StaticViewsController> _logger;

        public StaticViewsController(ApplicationContext context,
            IHostingEnvironment environment,
            IConfiguration configuration)
        {
            db = context;
            _environment = environment;
            _configuration = configuration;
            //_logger = 
        }

        public IActionResult Index()
        {
            return View(db.StaticViews.ToArray());
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int Id)
        {
            var staticView = db.StaticViews.Find(Id);
            if (staticView == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти представление для изменения.";
                return RedirectToAction("Index");
            }
            
            EditStaticViewModel model = new EditStaticViewModel
            {
                Content = ReadFromFile(staticView.Path),
                Id =  staticView.Id,
                Name = staticView.Name,
                Route = staticView.Route
            };
            
            return View(model);
        }

        public async Task<IActionResult> Remove(int Id)
        {
            var view = db.StaticViews.Find(Id);
            if (view == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти удаляемое представление.";
                return RedirectToAction("Index");
            }
            
            return View(view);
        }
        

        [HttpPost]
        public async Task<IActionResult> Create(CreateStaticViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string routeEnd = model.Route.Split('/').Last();
            if (!string.IsNullOrWhiteSpace(routeEnd))
            {
                ModelState.AddModelError("Route", "Не указано имя файла. (Имя файла = последнее слово маршрута не считая \"/\").");
                return View(model);
            }

            FileInfo staticView = new FileInfo(_environment.WebRootPath + $"/Generic/{model.Route}.html");
            if (staticView.Exists)
            {
                ModelState.AddModelError("Route", "Данный файл уже существует.");
                return View(model);
            }

            string[] dirs = model.Route.Split('/');
            if (dirs.Length > 1)
            {
                string fullPath = _environment.WebRootPath + "/Generic/";
                for (int i = 0; i < dirs.Length - 1; i++)
                {
                    fullPath += (dirs[i] + "/");
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);
                }
            }

            staticView.Create();
            if (WriteToFile(model.Content, staticView.FullName))
            {
                StaticView view = new StaticView
                {
                    Name = model.Name,
                    Route = model.Route,
                    Path = staticView.FullName
                };

                await db.StaticViews.AddAsync(view);
                await db.SaveChangesAsync();
            }
            HttpContext.Items["Success"] = "";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditStaticViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string routeEnd = model.Route.Split('/').Last();
            if (!string.IsNullOrWhiteSpace(routeEnd))
            {
                ModelState.AddModelError("Route", "Не указано имя файла. (Имя файла = последнее слово маршрута не считая \"/\").");
                return View(model);
            }

            StaticView view = db.StaticViews.FirstOrDefault(x => x.Id == model.Id);
            if (view == null)
                return NotFound("Указанная статическая страница не найдена.");

            FileInfo staticView = new FileInfo(_environment.WebRootPath + $"/Generic/{model.Route}.html");
            if (view.Path != $"/Generic/{model.Route}.html")
            {
                if (staticView.Exists)
                {
                    ModelState.AddModelError("Route", "Данный файл уже существует.");
                    return View(model);
                }

                string[] dirs = model.Route.Split('/');
                if (dirs.Length > 1)
                {
                    string fullPath = _environment.WebRootPath + "/Generic/";
                    for (int i = 0; i < dirs.Length - 1; i++)
                    {
                        fullPath += (dirs[i] + "/");
                        if (!Directory.Exists(fullPath))
                            Directory.CreateDirectory(fullPath);
                    }
                }

                staticView.Create();
            }

            if(WriteToFile(model.Content, staticView.FullName))
            {
                view.Name = model.Name;
                view.Path = staticView.FullName;
                view.Route = model.Route;

                db.StaticViews.Update(view);
                await db.SaveChangesAsync();
            }
            HttpContext.Items["SuccessMessage"] = "";
            return RedirectToAction("Index");
        }

        private bool WriteToFile(string content, string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(content);
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        [HttpPost]
        [ActionName("Remove")]
        public async Task<IActionResult> RemoveView(int Id)
        {
            StaticView view = db.StaticViews.FirstOrDefault(x => x.Id == Id);
            if (view == null)
            {
                HttpContext.Items["ErrorMessage"] = "Данная статическая страница не найдена.";
                return RedirectToAction("Index");
            }
                

            FileInfo file = new FileInfo(view.Path);
            if (file.Exists)
                file.Delete();

            db.StaticViews.Remove(view);
            await db.SaveChangesAsync();

            HttpContext.Items["SuccessMessage"] = "Статическая страница успешно удалена.";
            return RedirectToAction("Index");
        }
    }
}