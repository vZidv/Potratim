using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace src.Services
{
    public interface ICategoryService
    {
        public Task<Category> GetCategoryAsync(int id);
        public Task<List<Category>> GetAllCategoriesAsync();
        public Task<List<Category>> GetSomeRandCategoriesAsync(int count);

        public Task<Category> CreateCategoryAsync(string name);
        public Task<Category> UpdateCategoryAsync(Category category);
        public Task<bool> DeleteCategoryAsync(int id);
    }
}