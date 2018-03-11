using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Http;

using DNetCMS.Models.DataContract;

using SixLabors.ImageSharp;

namespace DNetCMS.Modules.Processing
{
    public class FileProcessing
    {
        //TODO: Add logging

        /// <summary>
        /// Automaticly recognize file type and redirect to UploadFile method
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <param name="rootPath">Application root path</param>
        /// <param name="db">Database instance</param>
        /// <returns>Call result</returns>
        internal static async Task<bool> UploadFile(IFormFile file, string rootPath, ApplicationContext db)
        {
            Enums.FileType fileType = GetFileType(file);

            return await UploadFile(file, fileType, rootPath, db);
        }

        /// <summary>
        /// Upload file to the server storage and database
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <param name="fileType">Target use</param>
        /// <param name="rootPath">Application root path</param>
        /// <param name="db">Database instance</param>
        /// <returns>Success of operation</returns>
        internal static async Task<bool> UploadFile(IFormFile file, Enums.FileType fileType, string rootPath, ApplicationContext db)
        {
            if (file == null)
                return false;

            FileModel result = new FileModel
            {
                FileType = (int)fileType,
                Name = file.FileName
            };

            switch (fileType)
            {
                case Enums.FileType.Document:
                    result.Path = "/Documents/" + file.FileName;
                    break;
                case Enums.FileType.Picture:
                    result.Path = "/Images/" + file.FileName;
                    break;
                case Enums.FileType.ToStore:
                default:
                    result.Path = "/Storage/" + file.FileName;
                    break;
            }

            using (var fileStream = new FileStream(rootPath+ result.Path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            await db.Files.AddAsync(result);
            await db.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Remove file from server
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        internal static async Task<bool> RemoveFile(string path, string rootPath)
        {
            try
            {
                FileInfo file = new FileInfo(rootPath + path);
                if (file.Exists)
                    file.Delete();

                return true;
            }
            catch (Exception e)
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
                    if (ImageCheck(file))
                        return Enums.FileType.Picture;
                    else
                        return Enums.FileType.ToStore;
                
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
    }
}
