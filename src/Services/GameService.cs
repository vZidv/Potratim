using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace src.Services
{
    public class GameService : IGameService
    {
        private readonly PotratimDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GameService(
            PotratimDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<Game?> GetGameAsync(Guid id)
        {
            if (id == null)
                throw new ArgumentNullException();

            string strId = id.ToString();
            return await GetGameAsync(strId);
        }
        public async Task<Game?> GetGameAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException();

            var game = await _context.Games
            .Include(g => g.Categories)
            .Include(g => g.Transactions)
            .Include(g => g.Reviews)
            .ThenInclude(r => r.User)
            .Where(g => g.Id.ToString() == id).FirstOrDefaultAsync();

            return game;
        }

        public async Task<Game> CreateGameAsync(CreateGameViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException();

            var imageUrl = await SaveGameImageAsync(model.ImageFile, FormatFileName(model.Title));
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

            if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
            {
                var selectedCategories = await _context.Categories.Where(c => model.SelectedCategoryIds.Contains(c.Id)).ToListAsync();
                game.Categories = selectedCategories;
            }

            _context.Games.Add(game);
            _context.SaveChanges();

            return game;
        }

        private async Task<string?> SaveGameImageAsync(IFormFile file, string fileName)
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

        public async Task<Game> UpdateGameAsync(EditGameViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException();

            var game = await GetGameAsync(model.Id);

            game.Title = model.Title;
            game.Description = model.Description;
            game.ReleaseDate = model.ReleaseDate;
            game.Developer = model.Developer;
            game.Publisher = model.Publisher;
            game.Price = model.Price;

            if (model.ImageFile != null)
            {
                var imageUrl = await SaveGameImageAsync(model.ImageFile, FormatFileName(model.Title));
                game.ImageUrl = imageUrl;
            }

            var selectedCategory = await _context.Categories.Where(c => model.SelectedCategoryIds.Contains(c.Id)).ToListAsync();
            for (int i = 0; i < game.Categories.Count; i++)
            {
                var category = game.Categories[i];
                if (!selectedCategory.Contains(category))
                {
                    game.Categories.RemoveAt(i);
                    i--;
                }
            }

            foreach (var category in selectedCategory)
            {
                if (!game.Categories.Contains(category))
                {
                    game.Categories.Add(category);
                }
            }

            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<bool> DeleteGameAsync(Guid id)
        {
            if (id == null)
                throw new ArgumentNullException();

            var game = await GetGameAsync(id);
            if (!String.IsNullOrWhiteSpace(game.ImageUrl))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, game.ImageUrl);
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<GameViewModel>> GetSimilarGamesAsync(string id, int count)
        {
            var game = await GetGameAsync(id);

            var similarGames = await _context.Categories.Include(c => c.Games)
                .Where(c => game.Categories.Select(gc => gc.Name).Contains(c.Name))
                .SelectMany(c => c.Games)
                .Where(g => g.Id != game.Id)
                .Take(count)
                .ToHashSetAsync();

            return similarGames.Select(g => GameToGameViewModel(g)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">Count must be: count > 0 and count <= 100</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<List<GameViewModel>> GetSomeGamesAsync(int count)
        {
            if (count <= 0 || count > 100)
                throw new ArgumentException();

            var someGames = await _context.Games.Take(count).ToListAsync();

            return someGames.Select(g => GameToGameViewModel(g)).ToList();
        }

        public GameViewModel GameToGameViewModel(Game game)
        {
            if (game == null)
                throw new ArgumentNullException();

            return new GameViewModel()
            {
                Id = game.Id,
                Title = game.Title,
                ReleaseDate = game.ReleaseDate,
                Price = game.Price,
                ImageUrl = game.ImageUrl,
                Categories = game.Categories.ToList()
            };
        }
    }
}