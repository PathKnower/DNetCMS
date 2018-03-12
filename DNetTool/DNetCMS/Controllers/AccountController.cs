using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels;
using DNetCMS.Models.ViewModels.Account;

namespace DNetCMS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        //TODO: написать логику пользователей.

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
        private async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            _logger.LogInformation("Change password action started");
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

        

    }
}