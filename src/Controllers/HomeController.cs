using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;
using src.Services;

namespace Potratim.Controllers;

public class HomeController : Controller
{
    private readonly PotratimDbContext _context;
    private readonly IGameService _gameService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        PotratimDbContext context,
        IGameService gameService,
        ICategoryService categoryService,
        ILogger<HomeController> logger)
    {
        _context = context;
        _gameService = gameService;
        _categoryService = categoryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Load home page");
            var viewModel = new HomeIndexViewModel();

            viewModel.NewGames = GetNewGamesList(8);
            viewModel.CategoriesGames = await GetGamesCategoriesList(2);
            viewModel.Categories = await _categoryService.GetSomeRandCategoriesAsync(10);
            viewModel.SomeGames = await _gameService.GetSomeGamesAsync(15);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View("Error");
        }
    }

    public async Task<IActionResult> ToCategory(int id)
    {
        try
        {
            _logger.LogInformation($"Redirecting to category with ID: {id}");
            Category category = await _context.Categories.FindAsync(id)!;
            CatalogIndexViewModel viewModel = new()
            {
                Categories = new List<Category> { category }
            };
            return RedirectToAction("Index", "Catalog", new { SelectedCategoriesId = new List<int> { category.Id } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error redirecting to category with ID: {id}");
            return NotFound();
        }

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public List<GameViewModel> GetNewGamesList(int count)
    {
        _logger.LogDebug("Getting new games list");

        return _context.Games.OrderByDescending(g => g.ReleaseDate)
            .Select(g => new GameViewModel
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl
            })
            .Take(count)
            .ToList();
    }

    public async Task<List<List<GameViewModel>>> GetGamesCategoriesList(int count)
    {
        _logger.LogDebug("Getting games categories list");

        var result = new List<List<GameViewModel>>();
        var categories = await _categoryService.GetAllCategoriesAsync();

        HashSet<int> selectedIndexes = new();
        Random random = new();
        for (int i = 0; i < count;)
        {
            int number = random.Next(0, categories.Count);
            if (selectedIndexes.Add(number))
            {
                if (categories[number].Games.Any())
                {
                    result.Add(categories[number].Games.Select(g => new GameViewModel
                    {
                        Id = g.Id,
                        Title = g.Title,
                        ReleaseDate = g.ReleaseDate,
                        Price = g.Price,
                        ImageUrl = g.ImageUrl,
                        Categories = new List<Category> { categories[number] }
                    }).Take(10)
                .ToList());
                    i++;
                }
            }
        }
        return result;
    }
}


