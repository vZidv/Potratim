using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;
using src.Services;
using src.ViewModel;
using X.PagedList.Extensions;

namespace Potratim.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GamesController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly IGameService _gameService;

        public GamesController(
        PotratimDbContext context,
        IGameService gameService)
        {
            _context = context;
            _gameService = gameService;
        }

        public IActionResult Index(
            int? page,
            string? searchString,
            int? minPrice,
            int? maxPrice,
            DateTime? dateFrom,
            DateTime? dateTo
            )
        {

            int pageSize = 10;
            int pageNumber = page ?? 1;

            IQueryable<Game> query = _context.Games;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(g => g.Title.Contains(searchString));
            }
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                query = query.Where(g => g.ReleaseDate >= dateFrom.Value && g.ReleaseDate <= dateTo.Value);
            }
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                query = query.Where(g => g.Price >= minPrice.Value && g.Price <= maxPrice.Value);
            }

            var games = query.ToPagedList(pageNumber, pageSize);

            var viewModel = new GamesIndexViewModel()
            {
                Games = games,
                SearchTerm = searchString,
                PriceMin = minPrice,
                PriceMax = maxPrice,
                ReleaseDateFrom = dateFrom,
                ReleaseDateTo = dateTo
            };

            return View(viewModel);
        }
        /////////////////////////////////////////////////////////

        public async Task<IActionResult> Create()
        {
            var model = new CreateGameViewModel();

            model.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGameViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _gameService.CreateGameAsync(model);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex);
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteConfirmed(Guid Id)
        {
            await _gameService.DeleteGameAsync(Id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(Guid Id)
        {
            var game = await _gameService.GetGameAsync(Id);
            if (game == null)
            {
                return NotFound();
            }
            EditGameViewModel editGame = new()
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                ReleaseDate = game.ReleaseDate,
                Developer = game.Developer,
                Publisher = game.Publisher,
                Price = game.Price,
                CurrentImageUrl = game.ImageUrl,

            };

            editGame.AllCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            editGame.CurrentCategoryIds = game.Categories.Select(c => c.Id).ToList();
            return View(editGame);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(EditGameViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _gameService.UpdateGameAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}