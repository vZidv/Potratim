using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Potratim.Data;
using Potratim.Models;

namespace src.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly PotratimDbContext _context;
        public CategoryService(
            PotratimDbContext context
        )
        {
            _context = context;
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.Include(c => c.Games).OrderBy(c => c.Name).ToListAsync();
        }
        public async Task<Category> GetCategoryAsync(int id)
        {
            if (id == null || id <= 0)
                throw new ArgumentNullException();

            return await _context.Categories
            .Include(c => c.Games)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
        }
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            if (id == null || id <= 0)
                throw new ArgumentNullException();

            Category category = await GetCategoryAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return true;
        }

        public async Task<List<Category>> GetSomeRandCategoriesAsync(int count)
        {
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
                throw new ArgumentNullException();

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> CreateCategoryAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException();

            Category newCategory = new() { Name = name };
            await _context.Categories.AddAsync(newCategory);
            await _context.SaveChangesAsync();

            return newCategory;
        }
    }
}