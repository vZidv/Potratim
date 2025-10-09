using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;
using src.ViewModel;

namespace Potratim.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly PotratimDbContext _context;

        public CategoriesController(PotratimDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
        string? searchString,
        string? sortOrder
        )
        {
            IQueryable<Category> categories = _context.Categories.Include(c => c.Games);
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => EF.Functions.ILike(c.Name, $"%{searchString}%"));
            }
            switch (sortOrder)
            {
                case "name":
                    categories = categories.OrderBy(c => c.Name);
                    break;
                case "games_count":
                    categories = categories.OrderByDescending(c => c.Games.Count());
                    break;
                default:
                    categories = categories.OrderBy(c => c.Id);
                    break;
            }
            var viewModel = new CategoriesIndexViewModel()
            {
                Categories = categories,
                SearchString = searchString,
                SortOrder = sortOrder
            };
            return View(viewModel);
        }

        /////////////////////////////////////////////////////////////////

        //Get 
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.AddAsync(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении. Попробуйте снова.");
                }
            }
            return View(category);
        }

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении. Попробуйте снова.");
                }
            }
            return RedirectToAction("Index");
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}