using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Http;

using DNetCMS.Models.DataContract;

using SixLabors.ImageSharp;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace DNetCMS.Modules.Processing
{
    public class FileProcessing
    {
        private ApplicationContext db;
        private IHostingEnvironment _environment;
        private ILogger<FileProcessing> _logger;
        
        public FileProcessing(ApplicationContext context,
            IHostingEnvironment environment,
            ILogger<FileProcessing> logger)
        {
            db = context;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Automaticly recognize file type and redirect to UploadFile method
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <returns>Call result</returns>
        public async Task<int> UploadFile(IFormFile file)
        {
            Enums.FileType fileType = GetFileType(file);

            return await UploadFile(file, fileType);
        }

        /// <summary>
        /// Upload file to the server storage and database
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <param name="fileType">Target use</param>
        /// <returns>Success of operation</returns>
        public async Task<int> UploadFile(IFormFile file, Enums.FileType fileType)
        {
            if (file == null)
                return -1;
            
            _logger.LogTrace($"Upload file method stared with filetype = {fileType.ToString()} and filename = {file.FileName}");
            _logger.LogTrace("Compute hash from file");
            string hash = GetHashFromFile(file.OpenReadStream());
            string dir1, dir2;

            _logger.LogTrace($"New file hash = {hash}");
            _logger.LogTrace("Determine path for new file");
            switch (fileType)
            {
                case Enums.FileType.Document:
                    dir1 = _environment.WebRootPath + "/Files/Documents/";
                    break;
                case Enums.FileType.Picture:
                    dir1 = _environment.WebRootPath + "/Files/Picture/";
                    break;
                case Enums.FileType.ToStore:
                default:
                    dir1 = _environment.WebRootPath + "/Files/Storage/";
                    break;
            }
            
            dir1 += $"{hash.Substring(0, 2)}";
            dir2 = $"{dir1}/{hash.Substring(2, 2)}/";
            
            _logger.LogTrace($"End directory path = {dir2}");
            _logger.LogTrace("Create directories");
            if (!Directory.Exists(dir1))
            {
                Directory.CreateDirectory(dir1);
                Directory.CreateDirectory(dir2);
            }
            else if (!Directory.Exists(dir2))
                Directory.CreateDirectory(dir2);

            FileModel result = new FileModel
            {
                FileType = (int)fileType,
                Name = file.FileName,
                Path = dir2 + file.FileName
            };
            _logger.LogTrace("Create file entity");
            _logger.LogTrace("Try to save file on disk");

            using (var fileStream = new FileStream(_environment.WebRootPath + result.Path, FileMode.Create))
            {
                try
                {
                    await file.CopyToAsync(fileStream);
                }
                catch (Exception e)
                {
                    _logger.LogError($"UploadFile method with file = {file.FileName}, hash = {hash} \n and final" +
                                     $"path = {_environment.WebRootPath + result.Path} was thrown exception = {e.Message} \n " +
                                     $"with stack trace = {e.StackTrace}");
                    throw;
                }
                
            }

            await db.Files.AddAsync(result);
            await db.SaveChangesAsync();

            return result.Id;
        }

        public async Task<int> AvatarSave(IFormFile file)
        {
            if (file == null || string.IsNullOrEmpty(_environment.WebRootPath))
                return 0;

            _logger.LogDebug($"AvatarSave action for file {file.FileName} started!");
            
            _logger.LogDebug("Check for image.");
            Image<Rgba32> src = Image.Load(file.OpenReadStream());
            _logger.LogDebug("Resize image to 128x128.");
            Image<Rgba32> image = src.Clone(x => x.Resize(new Size(128, 128)));
            
            _logger.LogDebug("Compute hash from new image.");
            string hash = GetHashFromFile(image.SavePixelData()); 
            
            _logger.LogDebug("Compute new image location.");
            string dir1 = _environment.WebRootPath + "/Files/Avatars/" + hash.Substring(0, 2);
            string dir2 = $"{dir1}/{hash.Substring(2, 2)}/";

            _logger.LogDebug("Check directory existance.");
            if (!Directory.Exists(dir1))
            {
                Directory.CreateDirectory(dir1);
                Directory.CreateDirectory(dir2);
            }
            else if (Directory.Exists(dir2))
                Directory.CreateDirectory(dir2);

            _logger.LogDebug("Start copy file to server.");
            string result = dir2 + file.FileName;
            image.Save(result);
            _logger.LogDebug($"File \"{file.FileName}\" save to path \"${result}\".");
            
            FileModel avatar = new FileModel
            {
                FileType = (int)Enums.FileType.Picture,
                Name = file.FileName,
                Path = result
            };
            
            _logger.LogDebug("Add new avatar to db.");
            await db.Files.AddAsync(avatar);
            await db.SaveChangesAsync();

            avatar = db.Files.FirstOrDefault(x => x.Path == avatar.Path);
            _logger.LogDebug("Successfull add avatar to db.");
            
            _logger.LogDebug("End of AvatarSave action.");
            return avatar.Id;
        }

        

        /// <summary>
        /// Remove file from server
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        internal static bool RemoveFile(string path, string rootPath)
        {
            try
            {
                FileInfo file = new FileInfo(rootPath + path);
                if (file.Exists)
                    file.Delete();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try to recognize file "Target"
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <returns></returns>
        internal static Enums.FileType GetFileType(IFormFile file)
        {
            string fileExtension = file.FileName.Split('.').Last();

            switch(fileExtension)
            {
                case "jpg":
                case "jpeg":
                case "jif":
                case "jfif":
                case "jfi":
                
                case "png":
                case "gif":
                case "svg":
                case "svgz":
                case "ico":
                    return ImageCheck(file) ? Enums.FileType.Picture : Enums.FileType.ToStore;
                
                //word
                case "doc":
                case "docx":
                case "docm":
                //excel
                case "xls":
                case "xlsx":
                case "xlsm":
                //power point
                case "ppt":
                case "pptx":
                case "pptm":
                //open office text doc
                case "sxw":
                case "stw":
                //open office spreadsheet 
                case "sxc":
                case "stc":
                //open office presentations
                case "sxi":
                case "sti":
                    return Enums.FileType.Document;

                default:
                    return Enums.FileType.ToStore;
            }
        }

        /// <summary>
        /// Check the image by create Image instance
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <returns>success of instance creation</returns>
        internal static bool ImageCheck(IFormFile file)
        {
            try
            {
                Image<Rgba32> img = Image.Load(file.OpenReadStream()); //if file is correctly open by image editor
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get hash from file stream by SHA1 alghoritm
        /// </summary>
        /// <param name="fileStream">File Stream</param>
        /// <returns></returns>
        internal static string GetHashFromFile(Stream fileStream)
        {
            var hash = SHA1.Create().ComputeHash(fileStream);
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
        
        private string GetHashFromFile(byte[] data)
        {
            var hash = SHA1.Create().ComputeHash(data);
            var sb = new StringBuilder(hash.Length * 2);

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
