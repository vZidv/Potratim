using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.ViewModel;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;
using X.PagedList.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Potratim.Models;

namespace Potratim.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly UserManager<User> _userManager;

        public UsersController(PotratimDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            int? page,
            string? roleSelect,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? searchString,
            string? status)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            IQueryable<User> query = _context.Users;

            //Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(u =>
                EF.Functions.ILike(u.Nickname, $"%{searchString}%") ||
                EF.Functions.ILike(u.Email, $"%{searchString}%") ||
                EF.Functions.ILike(u.Id.ToString(), $"%{searchString}%"));
            }

            //Фильтр роли
            if (!string.IsNullOrWhiteSpace(roleSelect))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleSelect);

                query = query.Where(u => _context.UserRoles.Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .Contains(u.Id));
            }

            //Проверка по дате регистрации
            if (dateFrom != null && dateTo != null)
            {
                query = query.Where(u => u.CreatedAt >= dateFrom && u.CreatedAt <= dateTo);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "active":
                        query = query.Where(u => u.LockoutEnd == null);
                        break;
                    case "blocked":
                        query = query.Where(u => u.LockoutEnd != null);
                        break;
                }
            }

            var users = query.ToPagedList(pageNumber, pageSize);

            var usersViewModel = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roles.FirstOrDefault());

                var userVM = new UserViewModel()
                {
                    Id = user.Id,
                    Nickname = user.Nickname,
                    Email = user.Email,
                    Status = user.LockoutEnd != null ? "Заблокирован" : "Активен",
                    AvatarUrl = user.ProfileImageUrl,
                    RoleName = userRole?.Name ?? "Empty",
                    RoleColor = userRole?.Color ?? "a6a6a6",
                    RegistrationDate = user.CreatedAt
                };
                usersViewModel.Add(userVM);
            }


            var rolesAll = await _context.Roles.ToListAsync();
            var viewModel = new UsersIndexViewModel()
            {
                Users = usersViewModel.ToPagedList(pageNumber, pageSize),
                Roles = rolesAll.Select(r => r.Name).ToList(),

                RoleFilter = roleSelect,
                StatusFilter = status,
                SearchTerm = searchString,
                RegistrationDateFrom = dateFrom,
                RegistrationDateTo = dateTo,


                AllUsersCount = usersViewModel.Count,
                ClientsCount = usersViewModel.Count(u => u.RoleName == "User"),
                ModeratorsCount = usersViewModel.Count(u => u.RoleName == "Moderator"),
                AdminsCount = usersViewModel.Count(u => u.RoleName == "Admin")
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(string id)
        {
            User? user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userRoleName = await _userManager.GetRolesAsync(user);
            UserRole? userRole = await _context.Roles.FirstOrDefaultAsync(ur => ur.Name == userRoleName.FirstOrDefault());
            var userViewModel = new UserViewModel()
            {
                Id = user.Id,
                Nickname = user.Nickname,
                Email = user.Email!,
                RoleName = userRole?.Name ?? "User",
                RoleColor = userRole?.Color ?? "607af7",
                Status = user.LockoutEnd != null ? "Заблокирован" : "Активен",
                AvatarUrl = user.ProfileImageUrl,
                RegistrationDate = user.CreatedAt
            };

            var roles = await _context.Roles.Select(r => r.Name).ToListAsync();

            var viewModel = new UserEditViewModel
            {
                User = userViewModel,
                Roles = roles!,
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(UserEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.User.Id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.Nickname = model.User.Nickname;
            user.Email = model.User.Email;

            if (!string.IsNullOrWhiteSpace(model.User.RoleName))
            {
                var userRole = await _userManager.GetRolesAsync(user);
                if(userRole.FirstOrDefault() != model.User.RoleName)
                {
                    await _userManager.AddToRoleAsync(user, model.User.RoleName);
                    await _userManager.RemoveFromRoleAsync(user, userRole.FirstOrDefault());
                }
            }

            switch (model.User.Status)
            {
                case "active":
                    user.LockoutEnd = null;
                    break;
                case "blocked":
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    break;
                default:
                    break;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<IActionResult> Ban(string id)
        {
            User user = await _userManager.FindByIdAsync(id);

            var lockoutEnd = DateTimeOffset.MaxValue;
            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }
        public async Task<IActionResult> Unban(string id)
        {
            User user = await _userManager.FindByIdAsync(id);

            var lockoutEnd = DateTimeOffset.MinValue;
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}