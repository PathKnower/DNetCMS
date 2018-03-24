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
using Microsoft.Extensions.Configuration;

namespace DNetCMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class RolesController : Controller
    {
        RoleManager<IdentityRole> _roleManager;
        UserManager<User> _userManager;
        IConfiguration _configuration;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IConfiguration configuration)
        {

            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        // public IActionResult Index() => View(_roleManager.Roles.ToList());

        public string Index() => _configuration.GetChildren().ToString();

        public IActionResult Create()
        {
            CreateRoleViewModel model = new CreateRoleViewModel
            {
                Claims = GetClaims()
            };

            return View(model);
        }


        public async Task<IActionResult> EditRoleClaimsAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound("Роль не найдена");

            ChangeRoleClaims model = new ChangeRoleClaims
            {
                Claims = GetClaims(),
                Id = id,
                Name = role.Name,
                SelectedAccessLevel = (await _roleManager.GetClaimsAsync(role))[0]?.Value
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            if (!string.IsNullOrEmpty(model.SelectedAccessLevel) || !GetClaims().Contains(model.SelectedAccessLevel))
            {
                ModelState.AddModelError("SelectedAccessLevel", "Выбранный уровень доступа не существует.");
                model.Claims = GetClaims();
                return View(model);
            }

            IdentityRole role = new IdentityRole(model.Name);

            IdentityResult result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

            result = await _roleManager.AddClaimAsync(role, new Claim("AccessLevel", model.SelectedAccessLevel));
            if(!result.Succeeded)
            {
                HttpContext.Items["RoleError"] = "Не удалось добавить к роли выбранный уровень доступа.";
            }   

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
                model.Claims = GetClaims();
                return View(model);
            }

            IdentityRole role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
                return NotFound("Изменяемая роль не найдена.");

            var claims = await _roleManager.GetClaimsAsync(role);
            if(claims.Where(x => x.Type == "AccessLevel").Select(x => x.Value).Contains(model.SelectedAccessLevel))
                return RedirectToAction("Index");

            if(claims.Count > 0)
                foreach(var claim in claims)
                    if(claim.Type == "AccessLevel")
                        await _roleManager.RemoveClaimAsync(role, claim);

            IdentityResult result = await _roleManager.AddClaimAsync(role, new Claim("AccessLevel", model.SelectedAccessLevel));
            if (!result.Succeeded)
            {
                HttpContext.Items["RoleError"] = "Не удалось добавить к роли выбранный уровень доступа.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);

            if (role != null)
            {   
                IdentityResult result = await _roleManager.DeleteAsync(role); //Так как каскадное удаление, то не нужно дополнительно писать удаление
            }
            return RedirectToAction("Index");
        }

        public IActionResult UserList() => View(_userManager.Users.ToList());

        public async Task<IActionResult> Edit(string userId)
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
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
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

                return RedirectToAction("UserList");
            }

            return NotFound();

        }

        private string[] GetClaims()
        {
            return new string[] { "Администратор", "Модератор", "Редактор" };
        }
    }
}