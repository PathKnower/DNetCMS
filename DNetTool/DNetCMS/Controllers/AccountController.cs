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
        private readonly FileProcessing _fileProcessing;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, 
            ApplicationContext context, 
            FileProcessing fileProcessing,
            IHostingEnvironment environment, 
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _db = context;
            _fileProcessing = fileProcessing;
            _appEnvironment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            return View(user);
        }
        
        [Route("[controller]/password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Route("[controller]/user-info")]
        public async Task<IActionResult> ChangeUserInfo()
        {
            User currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
        
            ChangeUserInfoViewModel model = new ChangeUserInfoViewModel
            {
                DateOfBirth = currentUser.DateOfBirth,
                FullName = currentUser.FullName
            };
            //Changge props like FirstName LastName Age and etc 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogDebug("Change password action started");

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                _logger.LogDebug("User not found with Email: {0}", User.Identity.Name);
                return NotFound("Пользователь не найден.");
            }

            if (!await _userManager.CheckPasswordAsync(user, model.OldPassword))
            {
                _logger.LogDebug("Old password is wrong");
                return BadRequest("Старый пароль неверен.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogDebug("ChangePasswordAsync method failed: {0}", result.Errors);
                return BadRequest("Не удалось сменить пароль.");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserInfo(ChangeUserInfoViewModel model)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
             _logger.LogDebug("ChangeUserInfo action started.");

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if(user == null)
            {
                _logger.LogDebug("User not found with Email: {0}", User.Identity.Name);
                return NotFound("Пользователь не найден.");
            }

            _logger.LogDebug("Change credetials.");
            user.FullName = model.FullName;
            user.DateOfBirth = model.DateOfBirth;
            

            if (model.Avatar != null)
            {
                _logger.LogDebug("Change avatar.");
                int id = await _fileProcessing.AvatarSave(model.Avatar);

                user.Avatar = await _db.Files.FindAsync(id);
            }
            await _userManager.UpdateAsync(user);
            _logger.LogDebug("ChangeUserInfo method finish successfull.");

            return RedirectToAction("Index");
        }

    }
}