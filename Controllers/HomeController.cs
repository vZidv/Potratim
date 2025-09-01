using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Potratim.Data;
using Potratim.Models;

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
        return View();
    }

    public async Task<IActionResult> SuperIndex()
    {
        try
        {
            Category category = new() { Id = 1, Name = "Tets Category" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            ViewBag.Message = "Категория успешно добавлена";
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Ошибка при работе с БД: {ex.Message}";
        }
        return View();
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
}
