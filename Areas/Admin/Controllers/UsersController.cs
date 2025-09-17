using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            string? roleSelect)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            IQueryable<User> query = _context.Users;

            if (!string.IsNullOrWhiteSpace(roleSelect))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleSelect);

                query = query.Where(u => _context.UserRoles.Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .Contains(u.Id));
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
                    Status = "Активен",
                    AvatarUrl = user.ProfileImageUrl,
                    RoleName = userRole?.Name ?? "User",
                    RoleColor = userRole?.Color ?? "607af7",
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

                AllUsersCount = usersViewModel.Count,
                ClientsCount = usersViewModel.Count(u => u.RoleName == "User"),
                ModeratorsCount = usersViewModel.Count(u => u.RoleName == "Moderator"),
                AdminsCount = usersViewModel.Count(u => u.RoleName == "Admin")
            };
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}