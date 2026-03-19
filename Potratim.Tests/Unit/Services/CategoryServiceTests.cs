using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Potratim.Models;
using src.Services;
using Xunit;

namespace Potratim.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ILogger<CategoryService>> _loggerMock;

        public CategoryServiceTests()
        {
            _loggerMock = new Mock<ILogger<CategoryService>>();
        }
        #region GetCategoryAsync Tests
        [Fact]
        public async Task GetAllCategoriesAsync()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var categoryes = new List<Category>
            {
                new Category { Name = "Action" },
                new Category { Name = "Adventure" },
                new Category { Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var categories = await service.GetAllCategoriesAsync();
            Assert.NotNull(categories);
            Assert.Equal(3, categories.Count);
            Assert.Contains(categories, c => c.Name == "Action");
            Assert.Contains(categories, c => c.Name == "Adventure");
            Assert.Contains(categories, c => c.Name == "RPG");

            DbContext.Dispose(db);
        }
        #endregion

        #region GetCategoryAsync Tests
        [Fact]
        public async Task GetCategoryAsync_ValidId()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);


            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var category2 = await service.GetCategoryAsync(2);
            var category3 = await service.GetCategoryAsync(3);

            Assert.NotNull(category2);
            Assert.NotNull(category3);

            Assert.Equal(2, category2.Id);
            Assert.Equal("Adventure", category2.Name);
            Assert.Equal(3, category3.Id);
            Assert.Equal("RPG", category3.Name);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task GetCategoryAsync_InvalidId_OverOfRange()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);


            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var exception = await Assert.ThrowsAsync<MyExceptions.CategoryNotFoundException>(() => service.GetCategoryAsync(999));

            Assert.Equal("Category with ID 999 not found", exception.Message);

            DbContext.Dispose(db);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetCategoryAsync_InvalidId(int id)
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);


            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetCategoryAsync(id));

            Assert.Equal("Id cannot be null and <= 0", exception.Message);
            Assert.Equal(nameof(id), exception.PropertyName);

            DbContext.Dispose(db);
        }
        #endregion

        #region DeleteCategoryAsync Tests
        [Fact]
        public async Task DeleteCategoryAsync_ValidId()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);


            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var result = await service.DeleteCategoryAsync(2);

            Assert.True(result);
            Assert.Null(db.Categories.Find(2));

            DbContext.Dispose(db);
        }
        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteCategoryAsync_InvalidId(int id)
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);


            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.DeleteCategoryAsync(id));

            Assert.Equal("Id cannot be null and <= 0", exception.Message);
            Assert.Equal(nameof(id), exception.PropertyName);

            DbContext.Dispose(db);
        }
        #endregion

        #region CreateCategoryAsync Tests
        [Theory]
        [InlineData("Strategy")]
        [InlineData("S1mulation")]
        public async Task CreateCategoryAsync_ValidData(string name)
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var category = await service.CreateCategoryAsync(name);

            Assert.NotNull(category);
            Assert.Equal(name, category.Name);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task CreateCategoryAsync_NullData()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.CreateCategoryAsync(null));

            Assert.Equal("Name cannot be null and white space", exception.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region UpdateCategoryAsync Tests
        [Fact]
        public async Task UpdateCategoryAsync_ValidData()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var categoryAdventure = new Category { Id = 2, Name = "Adventure" };
            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                categoryAdventure,
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();


            categoryAdventure.Name = "Updated Adventure";
            await service.UpdateCategoryAsync(categoryAdventure);

            var updatedCategory = db.Categories.Find(2);

            Assert.NotNull(updatedCategory);
            Assert.Equal("Updated Adventure", updatedCategory.Name);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task UpdateCategoryAsync_InvalidData()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var categoryAdventure = new Category { Id = 2, Name = "Adventure" };
            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                categoryAdventure,
                new Category { Id = 3, Name = "RPG" }
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();


            categoryAdventure.Name = "";
            var exception1 = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.UpdateCategoryAsync(categoryAdventure));
            var exception2 = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.UpdateCategoryAsync(null));

            Assert.Equal("Category cannot be null or category's name empty", exception1.Message);
            Assert.Equal("Category cannot be null or category's name empty", exception2.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetSomeRandCategoriesAsync Tests
        [Fact]
        public async Task GetSomeRandCategoriesAsync_ValidData()
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" },
                new Category { Id = 4, Name = "Strategy" },
                new Category { Id = 5, Name = "Shooters" },
                new Category { Id = 6, Name = "Survival" },
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var randomCategories = await service.GetSomeRandCategoriesAsync(3);

            Assert.NotNull(randomCategories);
            Assert.Equal(3, randomCategories.Count);

            Assert.NotEmpty(randomCategories[0].Name);

            DbContext.Dispose(db);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(null)]
        public async Task GetSomeRandCategoriesAsync_InvalidData(int count)
        {
            await using var db = DbContext.CreateInMemoryDbContext();
            var service = new CategoryService(db, _loggerMock.Object);

            var categoryes = new List<Category>
            {
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" },
                new Category { Id = 3, Name = "RPG" },
                new Category { Id = 4, Name = "Strategy" },
                new Category { Id = 5, Name = "Shooters" },
                new Category { Id = 6, Name = "Survival" },
            };
            db.Categories.AddRange(categoryes);
            db.SaveChanges();

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetSomeRandCategoriesAsync(count));

            Assert.Equal("Count cannot be null and <= 0", exception.Message);
            Assert.Equal(nameof(count), exception.PropertyName);

            DbContext.Dispose(db);
        }
        #endregion
    }
}