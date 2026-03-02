using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging;
using Moq;
using Potratim.Data;
using Potratim.Models;
using Potratim.MyExceptions;
using Potratim.ViewModel;
using src.Services;
using Xunit;

namespace Potratim.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<GameService>> _mockLogger;

        public GameServiceTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            var tempPath = Path.Combine(Path.GetTempPath(), "PotratimTestsTempFolder");
            Directory.CreateDirectory(tempPath);
            _mockEnv.Setup(e => e.WebRootPath).Returns(tempPath);

            _mockLogger = new Mock<ILogger<GameService>>();
        }

        #region GetGameAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetGameAsync_NullOrWhitespaceId(string id)
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetGameAsync(id));

            Assert.Equal(nameof(id), exception.PropertyName);
            DbContext.Dispose(db);
        }

        [Fact]
        public async Task GetGameAsync_ValidId_ReturnsGame()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            var id = Guid.NewGuid().ToString();
            var gameTitle = "Test Game";
            db.Games.Add(new Game
            {
                Id = Guid.Parse(id),
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            });
            await db.SaveChangesAsync();

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            var game = await service.GetGameAsync(id);

            Assert.NotNull(game);
            Assert.Equal(Guid.Parse(id), game.Id);
            Assert.Equal(gameTitle, game.Title);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task GetGameAsync_InvalidGameId()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            string invalidId = "invalid-guid";
            string expectedMessage = $"Invalid game ID format: {invalidId}";

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetGameAsync(invalidId));

            Assert.Contains(expectedMessage, exception.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task GetGameAsync_GameNotFound()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            var gameId = Guid.NewGuid();
            var expectedMessage = $"Game with ID {gameId} not found";
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            var exception = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => service.GetGameAsync(gameId.ToString()));

            Assert.Equal(gameId, exception.GameId);
            Assert.Contains(expectedMessage, exception.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task GetGameAsync_WithGuid()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            var gameId = Guid.NewGuid();
            var gameTitle = "Test Game";

            db.Games.Add(new Game
            {
                Id = gameId,
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            });
            await db.SaveChangesAsync();

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            var game = await service.GetGameAsync(gameId);

            Assert.NotNull(game);
            Assert.Equal(gameId, game.Id);
            Assert.Equal(gameTitle, game.Title);

            DbContext.Dispose(db);
        }
        #endregion

        #region CreateGameAsync Tests
        [Fact]
        public async Task CreateGameAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            var gameTitle = "Test Game";
            var viewModel = new CreateGameViewModel
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            };

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            var game = await service.CreateGameAsync(viewModel);

            Assert.NotNull(game);
            Assert.NotEmpty(game.Id.ToString());
            Assert.Equal(gameTitle, game.Title);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task CreateGameAsync_NullData()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            CreateGameViewModel? viewModel = null;

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            var exception = await Assert.ThrowsAnyAsync<MyExceptions.ValidationException>(() => service.CreateGameAsync(viewModel));

            Assert.Contains("Game model cannot be null", exception.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task CreateGameAsync_ValidData_WithCategories()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );
            await db.SaveChangesAsync();

            var gameTitle = "Test Game";
            var viewModel = new CreateGameViewModel
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300,
                SelectedCategoryIds = new List<int> { 1, 2 }
            };

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            var game = await service.CreateGameAsync(viewModel);

            Assert.NotNull(game);
            Assert.NotEmpty(game.Id.ToString());
            Assert.Equal(gameTitle, game.Title);
            Assert.NotNull(game.Categories);
            Assert.Equal(2, game.Categories.Count);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task CreateGameAsync_ValidData_WithImage()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );
            await db.SaveChangesAsync();

            var gameTitle = "Test Game";
            var imageFile = MakeTestFile();
            var viewModel = new CreateGameViewModel
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300,
                SelectedCategoryIds = new List<int> { 1, 2 },
                ImageFile = imageFile
            };

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            var game = await service.CreateGameAsync(viewModel);

            Assert.NotNull(game);
            Assert.NotEmpty(game.Id.ToString());
            Assert.Equal(gameTitle, game.Title);
            Assert.NotNull(game.Categories);
            Assert.Equal(2, game.Categories.Count);
            Assert.NotNull(game.ImageUrl);

            DbContext.Dispose(db);
        }

        private static IFormFile MakeTestFile(string fileName = "cover.png")
        {
            var content = new byte[] { 0x1, 0x2, 0x3 };
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, stream.Length, "image", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }
        #endregion

    }
}