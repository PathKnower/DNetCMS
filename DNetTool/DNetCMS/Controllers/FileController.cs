using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using DNetCMS.Models.DataContract;
using DNetCMS.Modules.Processing;
using DNetCMS.Models.ViewModels;

namespace DNetCMS.Controllers
{
    public class FileController : Controller
    {
        ApplicationContext db;
        IHostingEnvironment appEnvironment;


        public FileController(ApplicationContext context, IHostingEnvironment environment)
        {
            db = context;
            appEnvironment = environment;
        }

        public IActionResult Index(string message = "")
        {
            ViewBag.message = message;

            return View();
        }
        
        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(FileUploadViewModel model)
        {
            if (model.File == null)
                return View(model);

            bool result;

            if (string.IsNullOrEmpty(model.TargetUse))
                result =  await FileProcessing.UploadFile(model.File, appEnvironment.WebRootPath, db);
            else
            {
                Enums.FileType fileType;
                switch (model.TargetUse)
                {
                    case "Изображение":
                        fileType = Enums.FileType.Picture;
                        break;
                    case "Документ":
                        fileType = Enums.FileType.Document;
                        break;
                    case "Хранение":
                    default:
                        fileType = Enums.FileType.ToStore;
                        break;
                }

                result = await FileProcessing.UploadFile(model.File, fileType, appEnvironment.WebRootPath, db);
            }

            return RedirectToAction("Index", routeValues: "Файл загружен успешно.");
        }

        public IActionResult Delete(int id)
        {
            FileModel file = db.Files.FirstOrDefault(x => x.Id == id);

            if (file == null)
                return NotFound("Файл не найден.");

            return View(file);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> FileDelete(int id)
        {
            FileModel file = db.Files.FirstOrDefault(x => x.Id == id);

            if (file == null)
                return NotFound("Файл не найден.");

            bool success = await FileProcessing.RemoveFile(file.Path, appEnvironment.WebRootPath);

            if(success)
            {
                db.Files.Remove(file);
                await db.SaveChangesAsync();
            }
            else
            {
                return RedirectToAction("Index", routeValues: "Что-то пошло не так при удалении файла.");
            }

            return RedirectToAction("Index", routeValues: "Файл успешно удален.");
        }
    }
}