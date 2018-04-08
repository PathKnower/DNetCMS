using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Logging;

using DNetCMS.Models.DataContract;
using DNetCMS.Modules.Processing;
using DNetCMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "WriterAccess")]
    public class FilesController : Controller
    {
        private readonly ApplicationContext db;
        private readonly FileProcessing _fileProcessing;
        private readonly IHostingEnvironment appEnvironment;
        private readonly ILogger<FilesController> _logger;
        
        public FilesController(ApplicationContext context, 
            FileProcessing fileProcessing,
            IHostingEnvironment environment, 
            ILogger<FilesController> logger)
        {
            db = context;
            _fileProcessing = fileProcessing;
            appEnvironment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(db.Files.ToArray());
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

            int result;

            if (string.IsNullOrEmpty(model.TargetUse))
                result =  await _fileProcessing.UploadFile(model.File);
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

                result = await _fileProcessing.UploadFile(model.File, fileType);
            }

            if(result >= 0)
            {
                ModelState.AddModelError("CommonMessage", " Непредвиденная ошибка при загрузке аватара.");
                return View(model);
            }

            return RedirectToAction("Index");
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

            bool success = FileProcessing.RemoveFile(file.Path, appEnvironment.WebRootPath);

            if(success)
            {
                db.Files.Remove(file);
                await db.SaveChangesAsync();
            }
            else
            {
                //TODO: Общение с юзером
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}