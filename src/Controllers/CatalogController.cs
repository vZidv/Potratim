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
using Potratim.MyExceptions;
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
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(
            PotratimDbContext context,
            IGameService gameService,
            ICategoryService categoryService,
            ILogger<CatalogController> logger)
        {
            _context = context;
            _gameService = gameService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index
        (int? page,
        string? searchString,
        List<int>? selectedCategoriesId,
        int? minPrice,
        int? maxPrice,
        string? sortOrder)
        {
            _logger.LogInformation($"Loading catalog page");
            _logger.LogDebug($"Search String: {searchString}, Selected Categories: {string.Join(", ", selectedCategoriesId ?? new List<int>())}, Min Price: {minPrice}, Max Price: {maxPrice}, Sort Order: {sortOrder}");
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();

                int pageSize = 16;
                int pageNumber = page ?? 1;

                var queryAllGames = _context.Games
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
            catch (ValidationException ex)
            {
                _logger.LogError(ex, $"Invalid catalog filter parameters");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading catalog page");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}