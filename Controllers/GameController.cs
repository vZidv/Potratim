using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        public GameController(PotratimDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string id)
        {
            var game = await _context.Games.Include(g => g.Categories).Where(g => g.Id.ToString() == id).FirstOrDefaultAsync();

            var viewModel = new GameIndexViewModel()
            {
                Game = game,
                Categories = game.Categories.ToList(),
                SameGames = await GetSameGames(game.Id.ToString(), 12)
            };
            return View(viewModel);
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