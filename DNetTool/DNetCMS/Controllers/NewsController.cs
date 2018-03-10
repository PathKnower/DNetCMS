using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using DNetCMS.Models.DataContract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using DNetCMS.Models.ViewModels;
using System.IO;

namespace DNetCMS.Controllers
{
    public class NewsController : Controller
    {
        ApplicationContext db;
        IHostingEnvironment _appEnvironment;

        public NewsController(ApplicationContext context, IHostingEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }


        public IActionResult Index(string message = "")
        {
            ViewBag.Message = message;

            return View(db.News.ToArray());
        }
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string header = model.Header.Replace(".", string.Empty).Replace(",", string.Empty).Replace(" ", string.Empty).ToLower();

            News plagiat = await db.News.FirstOrDefaultAsync(x => x.Header.Replace(".", string.Empty).Replace(",", string.Empty).
            Replace(" ", string.Empty).ToLower() == header);

            if (plagiat != null)
            {
                ModelState.AddModelError("Plagiat", "Данный заголовок уже существует");
                return View(model);
            }

            News news = new News
            {
                Header = model.Header,
                Content = model.Content,
                CreateDate = DateTime.Now
            };

            if (model.Picture != null)
            {
                string path = "/Files/" + model.Picture.FileName;

                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await model.Picture.CopyToAsync(fileStream);
                }

                FileModel file = new FileModel { Name = model.Picture.FileName, Path = path };
                await db.Files.AddAsync(file);
                news.Picture = file;
            }

            await db.News.AddAsync(news);
            await db.SaveChangesAsync();

            return RedirectToAction("Index", routeValues: "Новость успешно добавлена.");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int newsId)
        {
            News news = await db.News.FirstOrDefaultAsync(x => x.Id == newsId);

            if (news == null)
                return RedirectToAction("Index");

            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody]NewsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            News news = await db.News.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (news == null)
                return NotFound("Исходная новость не найдена.");

            news.Content = model.Content;
            news.Header = model.Header;

            if (model.Picture != null)
            {
                string path = "/Files/" + model.Picture.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await model.Picture.CopyToAsync(fileStream);
                }
                FileModel file = new FileModel { Name = model.Picture.FileName, Path = path };
                db.Files.Add(file);
                news.Picture = file;
            }

            db.Entry(news).State = EntityState.Modified;

            await db.SaveChangesAsync();

            //NewsChanged();

            return RedirectToAction("Index", routeValues: "Новость успешно изменена.");
        }

        [HttpGet("newsId")]
        public async Task<IActionResult> Delete(int newsId)
        {
            News news = await db.News.FirstOrDefaultAsync(x => x.Id == newsId);

            if (news == null)
                return RedirectToAction("Index");

            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm([FromBody]int newsId)
        {
            News news = await db.News.FirstOrDefaultAsync(x => x.Id == newsId);

            if (news == null)
                return NotFound("Новость не найдена.");

            string filePath = news.Picture.Path;

            db.News.Remove(news);
            await db.SaveChangesAsync();

            FileInfo info = new FileInfo(filePath);

            if (info.Exists)
                info.Delete();

            //NewsChanged();

            return RedirectToAction("Index", routeValues: "Новость удалена.");
        }
    }
}