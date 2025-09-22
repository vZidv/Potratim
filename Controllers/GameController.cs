using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;

namespace Potratim.Controllers
{

    public class GameController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly UserManager<User> _userManager;

        public GameController(PotratimDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string id)
        {
            var game = await _context.Games.Include(g => g.Categories).Include(g => g.Reviews).Where(g => g.Id.ToString() == id).FirstOrDefaultAsync();

            var viewModel = new GameIndexViewModel()
            {
                Game = game,
                Categories = game.Categories.ToList(),
                SameGames = await GetSameGames(game.Id.ToString(), 12),
                Reviews = game.Reviews.ToList()
            };
            return View(viewModel);
        }
        public async Task<IActionResult> CreateReview(string gameId, string reviewText, bool reviewGrade)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);

            Review review = new()
            {
                Id = Guid.NewGuid(),
                GameId = Guid.Parse(gameId),
                UserId = user.Id,
                Comment = reviewText,
                Like = reviewGrade
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = gameId });
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