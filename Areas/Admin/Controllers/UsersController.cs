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

namespace Potratim.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly PotratimDbContext _context;

        public UsersController(PotratimDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();

            var viewModel = new UsersIndexViewModel()
            {
                AllUsersCount = users.Count,
                ClientsCount = users.Count(u => u.RoleId == roles.FirstOrDefault(r => r.Name == "User")?.Id),
                ModeratorsCount = users.Count(u => u.RoleId == roles.FirstOrDefault(r => r.Name == "Moderator")?.Id),
                AdminsCount = users.Count(u => u.RoleId == roles.FirstOrDefault(r => r.Name == "Admin")?.Id)
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