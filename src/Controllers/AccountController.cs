using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Potratim.Models;
using Potratim.ViewModel;
using Potratim.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Potratim.Services;

namespace Potratim.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly PotratimDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ICartService _cartService;
        private readonly ILogger<AccountController> _logger;

        public AccountController
        (PotratimDbContext context,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IWebHostEnvironment environment,
        ICartService cartService,
        ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _environment = environment;
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            _logger.LogInformation("Login page visited");
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return RedirectToAction("Profile", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Login attempted");
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        await _cartService.MergeCartAsync(HttpContext, user.Id);
                    }
                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    _logger.LogWarning("Login failed");
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Аккаунт заблокирован.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                }
            }


            return View(model);
        }


        public async Task<IActionResult> SignOut()
        {
            _logger.LogInformation("User initiated sign out");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User initiated logout");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("User registration attempted");
            if (ModelState.IsValid)
            {
                var username = model.Email.Split('@')[0];

                var user = new User
                {
                    UserName = username,
                    Email = model.Email,
                    CreatedAt = DateTime.Now,
                };


                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login", "Account");
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

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            _logger.LogInformation("User profile page visited");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Не удалось найти пользователя.");
            }
            var userRole = await _userManager.GetRolesAsync(user);
            var roleColor = _context.Roles.FirstOrDefault(r => r.Name == userRole.FirstOrDefault())?.Color;
            List<Game> games = _context.Users
            .Include(u => u.Games).Where(g => g.Id == user.Id)
            .SelectMany(u => u.Games).ToList();

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Nickname = user.UserName,
                Email = user.Email,
                RoleName = userRole.FirstOrDefault(),
                RoleColor = roleColor,
                Status = user.LockoutEnd != null ? "Заблокирован" : "Активен",
                RegistrationDate = user.CreatedAt,
                AvatarUrl = user.ProfileImageUrl
            };

            var gameCollection = games.Select(g => new GameViewModel
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl
            });

            var model = new UserProfileViewModel()
            {
                User = userViewModel,
                GameCollection = gameCollection.ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> EditProfile()
        {
            _logger.LogInformation("User profile edit page visited");
            var user = await _userManager.GetUserAsync(User);

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Nickname = user.UserName,
                Email = user.Email,
                RegistrationDate = user.CreatedAt,
                AvatarUrl = user.ProfileImageUrl
            };

            return View(userViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileConfirm(UserSelfEditViewModel model)
        {
            _logger.LogInformation("User profile edit confirmation attempted");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User profile edit confirmation failed");
                return View("EditProfile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found");
                return NotFound("Пользователь не найден.");
            }

            user.UserName = model.Nickname;
            user.Email = model.Email;
            if (model.AvatarFile != null)
            {
                user.ProfileImageUrl = await SaveFile(model.AvatarFile, FormatFileName(user.Id.ToString()));
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("EditProfile", model);
        }

        private async Task<string?> SaveFile(IFormFile file, string fileName)
        {
            _logger.LogInformation("File upload initiated");
            string fileUrl = null;

            if (file != null && file.Length > 0)
            {

                string uploadDir = Path.Combine(_environment.WebRootPath, "images", "avatars");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName + Path.GetExtension(file.FileName);

                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                fileUrl = $"/images/avatars/{uniqueFileName}";
            }

            return fileUrl;
        }

        private string FormatFileName(string fileName)
        {
            fileName = fileName.Trim().ToLower();
            fileName = fileName.Replace(" ", "_");
            fileName = Regex.Replace(fileName, @"[^a-z0-9_-]", "");
            return fileName;
        }

    }
}