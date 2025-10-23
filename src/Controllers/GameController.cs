using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.MyExceptions;
using Potratim.Services;
using Potratim.ViewModel;
using src.Services;

namespace Potratim.Controllers
{

    public class GameController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ICartService _cartService;
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(
            PotratimDbContext context,
            UserManager<User> userManager,
            ICartService cartService,
            IGameService gameService,
            ILogger<GameController> logger)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _gameService = gameService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string id)
        {
            _logger.LogInformation($"Loading game page for game ID: {id}");

            try
            {
                var game = await _gameService.GetGameAsync(id);
                User? user = await _userManager.GetUserAsync(User);
                string? userRole = user != null ? (await _userManager.GetRolesAsync(user)).FirstOrDefault() : null;

                List<ReviewViewModel> reviews = new();

                foreach (var r in game.Reviews)
                {
                    string role = (await _userManager.GetRolesAsync(r.User)).FirstOrDefault();
                    reviews.Add(new ReviewViewModel
                    {
                        User = new UserViewModel
                        {
                            Id = r.UserId,
                            Nickname = r.User.UserName,
                            AvatarUrl = r.User.ProfileImageUrl,
                            RoleName = role,
                            RoleColor = await _context.Roles.Where(r => r.Name == role).Select(r => r.Color).FirstOrDefaultAsync()
                        },
                        GameId = r.GameId,
                        Like = r.Like,
                        Comment = r.Comment
                    });
                }

                UserViewModel? currentUser = null;

                if (user != null)
                {
                    currentUser = new UserViewModel
                    {
                        Id = user.Id,
                        Nickname = user.UserName,
                        Email = user.Email,
                        RoleName = userRole,
                        RoleColor = await _context.Roles.Where(r => r.Name == userRole).Select(r => r.Color).FirstOrDefaultAsync(),
                        Status = user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow ? "Заблокирован" : "Активен",
                        AvatarUrl = user.ProfileImageUrl,
                        RegistrationDate = user.CreatedAt
                    };
                }
                var viewModel = new GameIndexViewModel()
                {
                    Game = game,
                    Categories = game.Categories.ToList(),
                    SameGames = await _gameService.GetSimilarGamesAsync(game.Id.ToString(), 12),
                    Reviews = reviews,
                    CreateReviewModel = new CreateReviewViewModel(),
                    CurrentUser = currentUser
                };
                return View(viewModel);
            }
            catch (GameNotFoundException ex)
            {
                _logger.LogError(ex, $"Game not found for ID: {id}");
                return NotFound("Такая игра не найдена");
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, $"Invalid game ID format: {id}");
                return BadRequest("Не верный формат идентификатора игры");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading game page for ID: {id}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(CreateReviewViewModel model)
        {
            _logger.LogInformation($"Creating review for game ID: {model.GameId}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid review model state for game ID: {model.GameId}");
                TempData["ErrorMessage"] = "Пожалуйста, исправьте ошибки в форме.";
                return RedirectToAction("Index", new { id = model.GameId });
            }

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);

            Review review = new()
            {
                Id = Guid.NewGuid(),
                GameId = model.GameId,
                UserId = user.Id,
                Comment = model.Comment,
                Like = model.Like
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ваш отзыв был успешно добавлен.";
            return RedirectToAction("Index", new { id = model.GameId });
        }
        public async Task<IActionResult> BuyNow(string id)
        {
            _logger.LogInformation($"Initiating buy now process for game ID: {id}");

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                await _cartService.AddToCartAsync(user.Id, Guid.Parse(id));
            }
            else
            {
                await _cartService.AddToCartAsync(HttpContext, Guid.Parse(id));
            }
            return RedirectToAction("Index", "Order");
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteReview(string id)
        {
            _logger.LogInformation($"Attempting to delete review for game with ID: {id}");
            var review = await _context.Reviews.FindAsync(Guid.Parse(id));
            if (review == null)
            {
                _logger.LogWarning($"Review not found for game with ID: {id}");
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = review.GameId });
        }
    }
}