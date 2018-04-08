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

using Microsoft.AspNetCore.Authorization;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "WriterAccess")]
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

            if (User.IsInRole("Admin"))
                return View(db.News.Include(x => x.Author).ToArray().Reverse());
            else
                return View(db.News.Include(x => x.Author).Where(x => x.Author.UserName == User.Identity.Name).ToArray().Reverse());
        }
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewsViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            string header = model.Header.Replace(".", string.Empty).Replace(",", string.Empty).Replace(" ", string.Empty).ToLower();

            News plagiat = await db.News.FirstOrDefaultAsync(x => x.Header.Replace(".", string.Empty).Replace(",", string.Empty).
            Replace(" ", string.Empty).ToLower() == header);

            if(plagiat != null)
            {
                ModelState.AddModelError("Plagiat", "Данный заголовок уже существует");
                return View(model);
            }
            
            News news = new News
            {
                Header = model.Header,
                Content = model.Content,
                Author = db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name),
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

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            News news = await db.News.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);

            if(news == null || news.Author.UserName != User.Identity.Name)
                return RedirectToAction("Index");

            NewsViewModel model = new NewsViewModel
            {
                Id = news.Id,
                Header = news.Header,
                Content = news.Content
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NewsViewModel model)
        {
            if(!ModelState.IsValid)
                return View(model);

            News news = await db.News.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == model.Id);
            
            if (news == null)
                return NotFound("Исходная новость не найдена.");

            if(!User.IsInRole("admin") && news.Author.UserName != User.Identity.Name)
                return BadRequest("Нельзя изменить чужую новость.");
            
                
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
                await db.Files.AddAsync(file);
                news.Picture = file;
            }
            
            db.Entry(news).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            News news = await db.News.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);

            if(news == null || news.Author.UserName != User.Identity.Name)
                return RedirectToAction("Index");
            
            return View(news);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            News news = await db.News.Include(x => x.Author).Include(x => x.Picture).FirstOrDefaultAsync(x => x.Id == id);

            if (news == null)
                return NotFound("Новость не найдена.");

            if(!User.IsInRole("admin") && news.Author.UserName != User.Identity.Name)
                return BadRequest("Нельзя удалить чужую новость.");
            
            db.News.Remove(news);
            await db.SaveChangesAsync();

            if(news.Picture != null)
            {
                FileInfo info = new FileInfo(_appEnvironment.WebRootPath + news.Picture.Path);

                if (info.Exists)
                    info.Delete();
            }

            return RedirectToAction("Index");
        }
    }
}