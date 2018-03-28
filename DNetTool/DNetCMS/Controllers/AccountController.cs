using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels.Account;
using DNetCMS.Modules.Processing;
using Microsoft.AspNetCore.Hosting;

namespace DNetCMS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationContext _db;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, ApplicationContext context, IHostingEnvironment environment, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _db = context;
            _appEnvironment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        

        public IActionResult ChangePassword()
        {
            return View();
        }


        public IActionResult ChangeAvatar()
        {
            return View();
        }

        public IActionResult ChangeUserInfo()
        {
            //Changge props like FirstName LastName Age and etc 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
           // _logger.LogInformation("Change password action started");
            if (!ModelState.IsValid)
            {
                //var message = GetMessageErrors(ModelState);
               // _logger.LogInformation("Incoming model is not valid: {0}", message);
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
               // _logger.LogInformation("User not found with Email: {0}", User.Identity.Name);
                return NotFound("Пользователь не найден.");
            }

            if (!await _userManager.CheckPasswordAsync(user, model.OldPassword))
            {
               // _logger.LogInformation("Old password is wrong");
                return BadRequest("Старый пароль неверен.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
               // _logger.LogInformation("ChangePasswordAsync method failed: {0}", result.Errors);
                return BadRequest("Не удалось сменить пароль.");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserInfo(ChangeUserInfoViewModel model)
        {
           // _logger.LogInformation("Change password action started");
            if (!ModelState.IsValid)
            {
                //var message = GetMessageErrors(ModelState);
                // _logger.LogInformation("Incoming model is not valid: {0}", message);
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if(user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.DateOfBirth = model.DateOfBirth;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeAvatar(ChangeAvatarViewModel model)
        {
            // _logger.LogInformation("Change password action started");
            if (!ModelState.IsValid)
            {
                //var message = GetMessageErrors(ModelState);
                // _logger.LogInformation("Incoming model is not valid: {0}", message);
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            int id = await FileProcessing.UploadFile(model.NewAvatar, Enums.FileType.Picture, _appEnvironment.WebRootPath, _db);

            user.Avatar = await _db.Files.FindAsync(id);
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

    }
}