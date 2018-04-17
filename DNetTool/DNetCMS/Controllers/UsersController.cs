using System.Linq;
using System.Threading.Tasks;
using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class UsersController : Controller
    {
        UserManager<User> _userManager;
        private ILogger<UsersController> _logger;

        public UsersController(UserManager<User> userManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult Create() => View();
        
        public async Task<IActionResult> Edit(string Id)
        {
            User user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                HttpContext.Items["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Index");
            }
            
            EditUserViewModel model = new EditUserViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }

        public async Task<IActionResult> Delete(string Id)
        {
            User user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                HttpContext.Items["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        //TODO: Переписать логику управления пользователями


        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogDebug("Successfully created user = {@user}", user);
                    HttpContext.Items["SuccessMessage"] = "Пользователь успешно создан.";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        _logger.LogDebug("Successfully update user = {id}", user.Id);
                        HttpContext.Items["SuccessMessage"] = "Изменения успешно сохранены.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    HttpContext.Items["SuccessMessage"] = "Пользователь успешно удален.";
                    _logger.LogInformation("Successfully delete user = {@id}, by admin = {admin}", user, User.Identity.Name);
                }
                else
                    HttpContext.Items["ErrorMessage"] = "Не удалось удалить пользователя.";
            }
            
            return RedirectToAction("Index");
        }

    }
}