using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Potratim.Data;
using Potratim.Models;
using Potratim.MyExceptions;

namespace src.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly PotratimDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            PotratimDbContext context,
            ILogger<CategoryService> logger
        )
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.Include(c => c.Games).OrderBy(c => c.Name).ToListAsync();
        }
        public async Task<Category> GetCategoryAsync(int id)
        {
            if (id == null || id <= 0)
                throw new ValidationException("Id cannot be null and <= 0")
                {
                    PropertyName = nameof(id),
                    AttemptedValue = id
                };

            _logger.LogDebug($"Retrieving category with ID {id}");

            var category = await _context.Categories
            .Include(c => c.Games)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

            if (category == null)
                throw new CategoryNotFoundException($"Category with ID {id} not found")
                {
                    CategoryId = id
                };

            return category;
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            _logger.LogDebug($"Deleting category with ID {id}");

            Category category = await GetCategoryAsync(id);
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return true;
        }

        public async Task<List<Category>> GetSomeRandCategoriesAsync(int count)
        {
            if (count == null || count <= 0)
                throw new ValidationException("Count cannot be null and <= 0")
                {
                    PropertyName = nameof(count),
                    AttemptedValue = count
                };

            _logger.LogDebug($"Retrieving random categories, count: {count}");

            var result = new List<Category>();
            Random random = new();

            var categories = await GetAllCategoriesAsync();
            for (int i = 0; i < count && i < categories.Count; i++)
            {
                int index = random.Next(0, categories.Count);
                result.Add(categories[index]);
                categories.RemoveAt(index);
            }
            return result;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
                throw new ValidationException("Category cannot be null or category's name empty")
                {
                    PropertyName = nameof(category),
                    AttemptedValue = category
                };

            _logger.LogDebug($"Updating category with ID {category.Id}");

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> CreateCategoryAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Name cannot be null and white space")
                {
                    PropertyName = nameof(name),
                    AttemptedValue = name
                };

            _logger.LogDebug($"Creating category with name {name}");

            Category newCategory = new() { Name = name };
            await _context.Categories.AddAsync(newCategory);
            await _context.SaveChangesAsync();

            return newCategory;
        }
    }
}