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

        public async Task<IActionResult> Index(int? page)
        {
            var categories = await _context.Categories.ToListAsync();
            int pageSize = 16;
            int pageNumber = page ?? 1;

            var games =  _context.Games
            .OrderByDescending(g => g.ReleaseDate)
            .Select(g => new GameViewModel()
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl,
                Categories = g.Categories
                
            })
            .ToPagedList(pageNumber, pageSize);

            var viewModel = new CatalogIndexViewModel()
            {
                Categories = categories,
                Games = games
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