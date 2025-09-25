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
using Potratim.Services;
using Potratim.ViewModel;

namespace Potratim.Controllers
{

    public class GameController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ICartService _cartService;

        public GameController(PotratimDbContext context, UserManager<User> userManager, ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index(string id)
        {
            var game = await _context.Games.Include(g => g.Categories).Include(g => g.Reviews).Where(g => g.Id.ToString() == id).FirstOrDefaultAsync();

            var viewModel = new GameIndexViewModel()
            {
                Game = game,
                Categories = game.Categories.ToList(),
                SameGames = await GetSameGames(game.Id.ToString(), 12),
                Reviews = game.Reviews.ToList(),
                CreateReviewModel = new CreateReviewViewModel()
            };
            return View(viewModel);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(CreateReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
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

        public async Task<List<GameViewModel>> GetSameGames(string id, int count)
        {
            var game = await _context.Games.Include(g => g.Categories).FirstOrDefaultAsync(g => g.Id.ToString() == id);
            var sameGames = await _context.Categories.Include(c => c.Games)
                .Where(c => game.Categories.Select(gc => gc.Name).Contains(c.Name))
                .SelectMany(c => c.Games)
                .Where(g => g.Id != game.Id)
                .Take(count)
                .ToHashSetAsync();

            return sameGames.Select(g => new GameViewModel()
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl,
                Categories = g.Categories.ToList()
            }).ToList();
        }
    }
}