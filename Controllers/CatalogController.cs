using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.ViewModel;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace Potratim.Controllers
{
    public class CatalogController : Controller
    {
        private readonly PotratimDbContext _context;
        public CatalogController(PotratimDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index
        (int? page,
        string? searchString,
        List<int>? selectedCategoriesId,
        int? minPrice,
        int? maxPrice)
        {
            var categories = await _context.Categories.ToListAsync();

            int pageSize = 16;
            int pageNumber = page ?? 1;


            var queryAllGames = _context.Games
            .OrderByDescending(g => g.ReleaseDate)
            .Select(g => new GameViewModel()
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl,
                Categories = g.Categories

            });

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                queryAllGames = queryAllGames.Where(g => EF.Functions.ILike(g.Title, $"%{searchString}%"));
            }
            if (selectedCategoriesId != null && selectedCategoriesId.Any())
            {
                queryAllGames = queryAllGames.Where(g =>
                g.Categories.Any(c => selectedCategoriesId.Contains(c.Id)) &&
            selectedCategoriesId.All(selectedId => 
                g.Categories.Any(c => c.Id == selectedId))
                );
            }
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                queryAllGames = queryAllGames.Where(g => (!minPrice.HasValue || g.Price >= minPrice) && (!maxPrice.HasValue || g.Price <= maxPrice));
            }

            var games = queryAllGames.ToPagedList(pageNumber, pageSize);

            var viewModel = new CatalogIndexViewModel()
            {
                Categories = categories,
                Games = games,

                //Filters
                SearchString = searchString,
                SelectedCategoriesId = selectedCategoriesId,
                MinPrice = minPrice ?? 0,
                MaxPrice = maxPrice ?? 15000
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