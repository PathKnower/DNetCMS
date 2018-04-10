﻿using System;
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

        public async Task<IActionResult> Index()
        {
            var list = new List<RoleViewModel>();
            foreach(var role in _roleManager.Roles.ToArray())
            {
                RoleViewModel model = new RoleViewModel
                {
                    Id = role.Id,
                    AccessLevel = (await _roleManager.GetClaimsAsync(role)).FirstOrDefault().Value,
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
                Claims = GetClaims()
            };

            return View(model);
        }

        public async Task<IActionResult> EditRole(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);

            if (role == null)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось найти роль.";
                return RedirectToAction("Index");
            }
                

            ChangeRoleClaims model = new ChangeRoleClaims
            {
                Claims = GetClaims(),
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
                if (role == null)
                {
                    HttpContext.Items["ErrorMessage"] = "Не удалось найти роль.";
                    return RedirectToAction("Index");
                }
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
                model.Claims = GetClaims();
                return View(model);
            }
                

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
                HttpContext.Items["WarningMessage"] = "Не удалось добавить к роли выбранный уровень доступа.";
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
            {
                HttpContext.Items["ErrorMessage"] = "Изменяемая роль не найдена.";
                return RedirectToAction("Index");
            }
                

            var claims = await _roleManager.GetClaimsAsync(role);
            //if(!claims.Where(x => x.Type == "AccessLevel").Select(x => x.Value).Contains(model.SelectedAccessLevel))
            //{
            //    ModelState.AddModelError("SelectedAccessLevel", "Выбранный уровень доступа не существует.");
            //    model.Claims = GetClaims();
            //    return View(model);
            //}

            role.Name = model.Name;

            if(claims.Count > 0)
                foreach(var claim in claims)
                    if(claim.Type == "AccessLevel")
                        await _roleManager.RemoveClaimAsync(role, claim);

            IdentityResult result = await _roleManager.AddClaimAsync(role, new Claim("AccessLevel", model.SelectedAccessLevel));
            if (!result.Succeeded)
            {
                HttpContext.Items["WarningMessage"] = "Не удалось добавить к роли выбранный уровень доступа.";
            }

            result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                HttpContext.Items["ErrorMessage"] = "Не удалось обновить роль.";
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
                if(result.Succeeded)
                    HttpContext.Items["SuccessMessage"] = "Удаление успешно завершено.";
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
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRole(string userId, List<string> roles)
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