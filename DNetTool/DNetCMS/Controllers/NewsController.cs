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
using Microsoft.Extensions.Logging;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "WriterAccess")]
    public class NewsController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly ILogger<NewsController> _logger;

        public NewsController(ApplicationContext context,
            IHostingEnvironment appEnvironment,
            ILogger<NewsController> logger)
        {
            _db = context;
            _appEnvironment = appEnvironment;
            _logger = logger;
        }


        public IActionResult Index()
        {
            News[] result;

            if (User.IsInRole("Admin"))
                result = _db.News.Include(x => x.Author).Include(x => x.Picture).ToArray();
            else
                result = _db.News.Include(x => x.Author).Where(x => x.Author.UserName == User.Identity.Name).ToArray();
            
            return View(result?.Reverse().ToArray());
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

            _logger.LogDebug("Create news action started!");
            _logger.LogTrace("Create news action started with model = {@model}", model);
            
            string header = model.Header.Replace(".", string.Empty).Replace(",", string.Empty).Replace(" ", string.Empty).ToLower();

            News plagiat = await _db.News.FirstOrDefaultAsync(x => x.Header.Replace(".", string.Empty).Replace(",", string.Empty).
            Replace(" ", string.Empty).ToLower() == header);

            if(plagiat != null)
            {
                ModelState.AddModelError("Plagiat", "Данный заголовок уже существует");
                return View(model);
            }
            
            _logger.LogDebug("Try create new news");
            News news = new News
            {
                Header = model.Header,
                Content = model.Content,
                Author = _db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name),
                CreateDate = DateTime.UtcNow
            };

            if (model.Picture != null)
            {
                _logger.LogDebug("Found image, try to add it");
                
                string path = "/Files/" + model.Picture.FileName;

                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await model.Picture.CopyToAsync(fileStream);
                }
                
                FileModel file = new FileModel { Name = model.Picture.FileName, Path = path };
                await _db.Files.AddAsync(file);
                news.Picture = file;
                _logger.LogTrace("Image successfully add with file = {@file}", file);
                _logger.LogDebug("Image successfully added");
            }
            
            await _db.News.AddAsync(news);
            await _db.SaveChangesAsync();
            _logger.LogDebug("Create news action finished!");

            HttpContext.Items["SuccessMessage"] = "Новость успешно добавлена!";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            News news = await _db.News.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);

            if (news == null || news.Author.UserName != User.Identity.Name && (User.HasClaim("AccessLevel", "ModeratorAccess")))
            {
                HttpContext.Items["ErrorMessage"] = (news == null)? "Новость не найдена." : "Вы не являетесь автором новости.";
                return RedirectToAction("Index");
            }

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

            News news = await _db.News.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == model.Id);

            if (news == null)
            {
                _logger.LogWarning("Editable news not found {model.Id}!", model.Id);
                HttpContext.Items["ErrorMessage"] = "Запрашиваемая новость не найдена";
                return RedirectToAction("Index");
            }


            if (!User.IsInRole("admin") && news.Author.UserName != User.Identity.Name)
            {
                _logger.LogWarning("User {User.Identity.Name} try to edit news {@news}", news);
                HttpContext.Items["ErrorMessage"] = "Вы не являетесь владельцем новости.";
                return RedirectToAction("Index");
            }

            _logger.LogDebug("Edit news action started!");
                
            news.Content = model.Content;
            news.Header = model.Header;

            if (model.Picture != null)
            {
                _logger.LogDebug("Try to upload image");
                string path = "/Files/" + model.Picture.FileName;

                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await model.Picture.CopyToAsync(fileStream);
                }

                FileModel file = new FileModel { Name = model.Picture.FileName, Path = path };
                await _db.Files.AddAsync(file);
                news.Picture = file;
                _logger.LogTrace("Image successfully add with file = {@file}", file);
                _logger.LogDebug("Image successfully added");
            }
            
            _db.News.Update(news);
            await _db.SaveChangesAsync();
            _logger.LogDebug("Create news action finished!");

            HttpContext.Items["SuccessMessage"] = "Новость успешно изменена!";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            News news = await _db.News.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);

            if (news == null || news.Author.UserName != User.Identity.Name)
            {
                
            }

            NewsViewModel model = new NewsViewModel
            {
                Content = news.Content,
                CreateDate = news.CreateDate,
                Header = news.Header,
                Id = news.Id
            };
            
            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            News news = await _db.News.Include(x => x.Author).Include(x => x.Picture).FirstOrDefaultAsync(x => x.Id == id);

            if (news == null)
            {
                _logger.LogWarning("Editable news not found {id}!", id);
                HttpContext.Items["ErrorMessage"] = "Запрашиваемая новость не найдена";
                return RedirectToAction("Index");
            }


            if (!User.IsInRole("admin") && news.Author.UserName != User.Identity.Name)
            {
                _logger.LogWarning("User {User.Identity.Name} try to edit news {@news}", news);
                HttpContext.Items["ErrorMessage"] = "Вы не являетесь владельцем новости";
                return RedirectToAction("Index");
            }
            
            _db.News.Remove(news);
            await _db.SaveChangesAsync();

            if(news.Picture != null)
            {
                FileInfo info = new FileInfo(_appEnvironment.WebRootPath + news.Picture.Path);

                if (info.Exists)
                {
                    info.Delete();
                    _logger.LogDebug("Successfull delete file from server");
                }
                
            }

            _logger.LogDebug("Delete news with id = {id}", id);
            HttpContext.Items["SuccessMessage"] = "Файл успешно удален";
            
            return RedirectToAction("Index");
        }
    }
}