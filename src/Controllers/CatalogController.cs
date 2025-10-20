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
using src.Services;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace Potratim.Controllers
{
    public class CatalogController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly IGameService _gameService;
        private readonly ICategoryService _categoryService;
        public CatalogController(
            PotratimDbContext context,
            IGameService gameService,
            ICategoryService categoryService)
        {
            _context = context;
            _gameService = gameService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index
        (int? page,
        string? searchString,
        List<int>? selectedCategoriesId,
        int? minPrice,
        int? maxPrice,
        string? sortOrder)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();

            int pageSize = 16;
            int pageNumber = page ?? 1;

            var queryAllGames = _context.Games
            .Select(g => _gameService.GameToGameViewModel(g));

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

            switch (sortOrder)
            {
                case "new":
                    {
                        queryAllGames = queryAllGames.OrderByDescending(g => g.ReleaseDate);
                    }
                    break;
                case "price_desc":
                    {
                        queryAllGames = queryAllGames.OrderByDescending(g => g.Price);
                    }
                    break;
                case "price_asc":
                    {
                        queryAllGames = queryAllGames.OrderBy(g => g.Price);
                    }
                    break;

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
                MaxPrice = maxPrice ?? 15000,
                SortOrder = sortOrder ?? "new"
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