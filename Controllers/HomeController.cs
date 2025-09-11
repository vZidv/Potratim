using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;

namespace Potratim.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PotratimDbContext _context;

    public HomeController(ILogger<HomeController> logger, PotratimDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var viewModel = new HomeIndexViewModel();

        viewModel.NewGames = GetNewGamesList(8);
        
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public List<GameViewModel> GetNewGamesList(int count)
    {
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
}
