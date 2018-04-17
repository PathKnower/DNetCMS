using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

using DNetCMS.Models.DataContract;
using DNetCMS.Models.ViewModels.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DNetCMS.Controllers
{
    //[Authorize(Policy = "AdminAccess")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<IdentityRole> roleManager, 
            UserManager<User> userManager, 
            IConfiguration configuration,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var list = new List<RoleViewModel>();
            foreach(var role in _roleManager.Roles.ToArray())
            {
                RoleViewModel model = new RoleViewModel
                {
                    Id = role.Id,
                    AccessLevel = (await _roleManager.GetClaimsAsync(role)).FirstOrDefault()?.Value,
                    Name = role.Name
                };
                list.Add(model);
            }

            return View(list);
        }

        public IActionResult Create()
        {
            CreateRoleViewModel model = new CreateRoleViewModel
            {
                Claims = new SelectList(GetClaims())
            };

            return View(model);
        }

        public async Task<IActionResult> Edit(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);

            if (role == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти роль.";
                return RedirectToAction("Index");
            }
                

            ChangeRoleClaims model = new ChangeRoleClaims
            {
                Claims = new SelectList(GetClaims()),
                Id = Id,
                Name = role.Name,
                SelectedAccessLevel = (await _roleManager.GetClaimsAsync(role))[0]?.Value
            };

            return View(model);
        }

        public async Task<IActionResult> Delete(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);
            if(role == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти роль.";
                return RedirectToAction("Index");
            }

            RoleViewModel model = new RoleViewModel
            {
                Id = role.Id,
                AccessLevel = (await _roleManager.GetClaimsAsync(role))[0]?.Value,
                Name = role.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Claims = new SelectList(GetClaims());
                return View(model);
            }
                
            
            if (string.IsNullOrEmpty(model.SelectedAccessLevel) || !GetClaims().Contains(model.SelectedAccessLevel))
            {
                ModelState.AddModelError("SelectedAccessLevel", "Выбранный уровень доступа не существует.");
                model.Claims = new SelectList(GetClaims());
                return View(model);
            }
            
            _logger.LogDebug("Create Role method started");
            var role = new IdentityRole(model.Name);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogDebug("Model state error");
                    model.Claims = new SelectList(GetClaims());
                    return View(model);
                }
           
            _logger.LogDebug("Successfull create role");

            _logger.LogDebug("Try to add claims");
            result = await _roleManager.AddClaimAsync(role, new Claim("AccessLevel", model.SelectedAccessLevel));
            if(!result.Succeeded)
            {
                HttpContext.Items["WarningMessage"] = "Не удалось добавить к роли выбранный уровень доступа.";
                _logger.LogDebug("Failed to add claims to role");
            }   
            _logger.LogInformation("Successfully create role {@role}", role);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ChangeRoleClaims model)
        {
            if (!ModelState.IsValid)
                return View();

            if (!string.IsNullOrEmpty(model.SelectedAccessLevel) || !GetClaims().Contains(model.SelectedAccessLevel))
            {
                ModelState.AddModelError("SelectedAccessLevel", "Выбранный уровень доступа не существует.");
                model.Claims = new SelectList(GetClaims());
                return View(model);
            }

            
            IdentityRole role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                HttpContext.Items["ErrorMessage"] = "Изменяемая роль не найдена.";
                _logger.LogError("Cannot find role with id = {model.Id}", model.Id);
                return RedirectToAction("Index");
            }
                
            _logger.LogDebug("Change role action started");
            var claims = await _roleManager.GetClaimsAsync(role);
            if(!claims.Where(x => x.Type == "AccessLevel").Select(x => x.Value).Contains(model.SelectedAccessLevel))
            {
                ModelState.AddModelError("SelectedAccessLevel", "Выбранный уровень доступа не существует.");
                model.Claims = new SelectList(GetClaims());
                return View(model);
            }

            role.Name = model.Name;
            _logger.LogDebug("Remove old claims");
            if(claims.Count > 0)
                foreach(var claim in claims)
                    if(claim.Type == "AccessLevel")
                        await _roleManager.RemoveClaimAsync(role, claim);

            IdentityResult result = await _roleManager.AddClaimAsync(role, new Claim("AccessLevel", model.SelectedAccessLevel));
            if (!result.Succeeded)
            {
                HttpContext.Items["WarningMessage"] = "Не удалось добавить к роли выбранный уровень доступа.";
                _logger.LogDebug("Cannot add claim AccessLevel to role");
            }

            result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                HttpContext.Items["SuccesssMessage"] = "Роль успешно изменена";
                _logger.LogInformation("Successfully update role {@role}", role);
            }
            else
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось обновить роль.";
                _logger.LogError("Cannot update role. Errors = {@Errors}", result.Errors);
            }
            

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);

            if (role != null)
            {   
                IdentityResult result = await _roleManager.DeleteAsync(role); //Так как каскадное удаление, то не нужно дополнительно писать удаление
                if (result.Succeeded)
                {
                    HttpContext.Items["SuccessMessage"] = "Удаление успешно завершено.";
                    _logger.LogInformation("Successfully delete role with id = {id}", id);
                }
                else
                {
                    HttpContext.Items["ErrorMessage"] = "";
                    _logger.LogError("Error to remove role. Errors = {@Errors}", result.Errors);
                }
                    
            }
            
            return RedirectToAction("Index");
        }

        public IActionResult UserList() => View(_userManager.Users.ToList());

        public async Task<IActionResult> EditUserRole(string userId)
        {
            // получаем пользователя
            
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                
                // получем список ролей пользователя

                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };

                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRole(string userId, List<string> roles)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                _logger.LogDebug("Edit user role action started");
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);

                // получаем все роли
                var allRoles = _roleManager.Roles.ToList();

                // получаем список ролей, которые были добавлены
                var addedRoles = roles.Except(userRoles);

                // получаем роли, которые были удалены
                var removedRoles = userRoles.Except(roles);

                await _userManager.AddToRolesAsync(user, addedRoles);
                await _userManager.RemoveFromRolesAsync(user, removedRoles);
                _logger.LogDebug("Successfully changed user role");

                HttpContext.Items["SuccessMessage"] = "Смена ролей пользователя прошла успешно";
                return RedirectToAction("UserList");
            }

            HttpContext.Items["ErrorMessage"] = "Пользователь не был найден";
            return RedirectToAction("UserList");

        }

        private string[] GetClaims()
        {
            return new string[] { "Администратор", "Модератор", "Редактор" };
        }
    }
}