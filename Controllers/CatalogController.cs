using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.ViewModel;

namespace Potratim.Controllers
{
    public class CatalogController : Controller
    {
        private readonly PotratimDbContext _context;
        public CatalogController(PotratimDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            var viewModel = new CatalogIndexViewModel()
            {
                Categories = categories
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