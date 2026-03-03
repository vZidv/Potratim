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

        [Theory]
        [InlineData("Resident Evil Requiem", "resident_evil_requiem")]
        [InlineData("DEATH STRANDING 2: ON THE BEACH DELUXE", "death_stranding_2_on_the_beach_deluxe")]
        [InlineData("Special@#$Chars!", "specialchars")]
        [InlineData("             SPACE             ", "space")]
        public async Task CreateGameAsync_ValidData_WithImage(string? fileName, string expected)
        {
            using var db = DbContext.CreateInMemoryDbContext();

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );
            await db.SaveChangesAsync();

            var gameTitle = fileName;
            var imageFile = MakeTestFile(fileName);
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
            Assert.Contains(expected, game.ImageUrl);

            DbContext.Dispose(db);
        }

        [Theory]
        [InlineData("        ")]
        [InlineData(null)]
        public async Task CreateGameAsync_InvalidData_WithImage(string? fileName)
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var expectedMessage = "fileName cannot be null or white space";

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );
            await db.SaveChangesAsync();

            var gameTitle = fileName;
            var imageFile = MakeTestFile(fileName);
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
            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.CreateGameAsync(viewModel));


            Assert.Contains(expectedMessage, exception.Message);

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

        #region DeleteGameAsync Tests
        [Fact]
        public async Task DeleteGameAsync_ValidData_WithImage_Use_CreateGameAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );
            await db.SaveChangesAsync();

            var gameTitle = "Test Game";
            var imageFile = MakeTestFile(gameTitle);
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

            await service.DeleteGameAsync(game.Id);
            Assert.Empty(db.Games.Where(g => g.Id == game.Id));

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task DeleteGameAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();


            var gameTitle = "Test Game";
            var game = new Game
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();

            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);
            await service.DeleteGameAsync(game.Id);

            Assert.Empty(db.Games.Where(g => g.Id == game.Id));

            DbContext.Dispose(db);
        }
        #endregion
        #region UpdateGameAsync Tests
        [Fact]
        public async Task UpdateGameAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            var gameTitle = "Test Game";
            var game = new Game
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();

            var gameEditModel = new EditGameViewModel
            {
                Id = game.Id,
                Title = "Updated Game",
                Description = "Updated Description",
                ReleaseDate = DateTime.Now.AddDays(1),
                Developer = "Updated Developer",
                Publisher = "Updated Publisher",
                Price = 400
            };
            await service.UpdateGameAsync(gameEditModel);
            var updatedGame = await db.Games.FindAsync(game.Id);

            Assert.NotNull(updatedGame);
            Assert.Equal(gameEditModel.Title, updatedGame.Title);
            Assert.Equal(gameEditModel.Description, updatedGame.Description);
            Assert.Equal(gameEditModel.ReleaseDate, updatedGame.ReleaseDate);
            Assert.Equal(gameEditModel.Developer, updatedGame.Developer);
            Assert.Equal(gameEditModel.Publisher, updatedGame.Publisher);
            Assert.Equal(gameEditModel.Price, updatedGame.Price);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task UpdateGameAsync_NullData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            var gameTitle = "Test Game";
            var game = new Game
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();


            var exception = Assert.ThrowsAsync<MyExceptions.ValidationException>(async () => await service.UpdateGameAsync(null));

            Assert.Contains("Game model cannot be null", exception.Result.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GameToGameViewModel Tests
        [Fact]
        public async Task GameToGameViewModel_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            var gameTitle = "Test Game";
            var game = new Game
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300
            };

            var gameViewModel = service.GameToGameViewModel(game);

            Assert.NotNull(gameViewModel);
            Assert.Equal(typeof(GameViewModel), gameViewModel.GetType());
            Assert.Equal(gameTitle, gameViewModel.Title);
            Assert.Equal(game.ReleaseDate, gameViewModel.ReleaseDate);
            Assert.Equal(game.Price, gameViewModel.Price);

            DbContext.Dispose(db);
        }
        #endregion
    }
}