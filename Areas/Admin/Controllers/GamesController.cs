using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;

namespace Potratim.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GamesController : Controller
    {
        private readonly PotratimDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GamesController(PotratimDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var games = _context.Games.ToList();
            return View(games);
        }
        /////////////////////////////////////////////////////////

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var imageUrl = await SaveFile(model.ImageFile, FormatFileName(model.Title));

                Game game = new()
                {
                    Title = model.Title,
                    Description = model.Description,
                    ReleaseDate = model.ReleaseDate,
                    Developer = model.Developer,
                    Publisher = model.Publisher,
                    Price = model.Price,
                    ImageUrl = imageUrl
                };
                _context.Games.Add(game);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid Id)
        {
            var game = await _context.Games.FindAsync(Id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Game game)
        {
            if (!String.IsNullOrWhiteSpace(game.ImageUrl))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, game.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(Guid Id)
        {
            var game = await _context.Games.FindAsync(Id);
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
                CurrentImageUrl = game.ImageUrl
            };
            return View(editGame);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(EditGameViewModel model)
        {
            if (ModelState.IsValid)
            {
                var game = await _context.Games.FindAsync(model.Id);
                if (game == null)
                {
                    return NotFound();
                }

                game.Title = model.Title;
                game.Description = model.Description;
                game.ReleaseDate = model.ReleaseDate;
                game.Developer = model.Developer;
                game.Publisher = model.Publisher;
                game.Price = model.Price;

                if (model.ImageFile != null)
                {
                    var imageUrl = await SaveFile(model.ImageFile, FormatFileName(model.Title));
                    game.ImageUrl = imageUrl;
                }

                _context.Games.Update(game);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        private async Task<string?> SaveFile(IFormFile file, string fileName)
        {
            string fileUrl = null;

            if (file != null && file.Length > 0)
            {

                string uploadDir = Path.Combine(_environment.WebRootPath, "images", "game-images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName + Path.GetExtension(file.FileName);

                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                fileUrl = $"/images/game-images/{uniqueFileName}";
            }

            return fileUrl;
        }

        private string FormatFileName(string fileName)
        {
            fileName = fileName.Trim().ToLower();
            fileName = fileName.Replace(" ", "_");
            fileName = Regex.Replace(fileName, @"[^a-z0-9_-]", "");
            return fileName;
        }
    }
}