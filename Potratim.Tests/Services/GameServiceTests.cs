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
    public class GameServiceTests : IDisposable
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<GameService>> _mockLogger;

        private readonly string _tempPath;

        public GameServiceTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempPath);
            _mockEnv.Setup(e => e.WebRootPath).Returns(_tempPath);

            _mockLogger = new Mock<ILogger<GameService>>();
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (Directory.Exists(_tempPath))
            {
                Directory.Delete(_tempPath, true);
            }
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
        [Fact]
        public async Task UpdateGameAsync_ValidData_With_Image()
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

            var imageFile = MakeTestFile(gameTitle);
            var gameEditModel = new EditGameViewModel
            {
                Id = game.Id,
                Title = "Updated Game",
                Description = "Updated Description",
                ReleaseDate = DateTime.Now.AddDays(1),
                Developer = "Updated Developer",
                Publisher = "Updated Publisher",
                Price = 400,
                ImageFile = imageFile
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
            Assert.Contains("updated_game", updatedGame.ImageUrl);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task UpdateGameAsync_ValidData_With_Image_And_Categoryes()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );

            var gameTitle = "Test Game";
            var game = new Game
            {
                Title = gameTitle,
                Description = "Test Description",
                ReleaseDate = DateTime.Now,
                Developer = "Test Developer",
                Publisher = "Test Publisher",
                Price = 300,
                Categories = new List<Category>() { }
            };
            db.Games.Add(game);
            await db.SaveChangesAsync();

            var imageFile = MakeTestFile(gameTitle);
            var gameEditModel = new EditGameViewModel
            {
                Id = game.Id,
                Title = "Updated Game",
                Description = "Updated Description",
                ReleaseDate = DateTime.Now.AddDays(1),
                Developer = "Updated Developer",
                Publisher = "Updated Publisher",
                Price = 400,
                ImageFile = imageFile,
                SelectedCategoryIds = new List<int> { 1, 2 }
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
            Assert.Contains("updated_game", updatedGame.ImageUrl);
            Assert.NotNull(updatedGame.Categories);
            Assert.Equal(2, updatedGame.Categories.Count);
            Assert.Contains(updatedGame.Categories, c => c.Name == "Action");

            gameEditModel = new EditGameViewModel
            {
                Id = game.Id,
                Title = "Updated Game",
                Description = "Updated Description",
                ReleaseDate = DateTime.Now.AddDays(1),
                Developer = "Updated Developer",
                Publisher = "Updated Publisher",
                Price = 400,
                ImageFile = imageFile,
                SelectedCategoryIds = new List<int> { 2 }
            };

            await service.UpdateGameAsync(gameEditModel);
            var secondUpdatedGame = await db.Games.FindAsync(game.Id);

            Assert.NotNull(secondUpdatedGame);
            Assert.Equal(gameEditModel.Title, secondUpdatedGame.Title);
            Assert.Equal(gameEditModel.Description, secondUpdatedGame.Description);
            Assert.Equal(gameEditModel.ReleaseDate, secondUpdatedGame.ReleaseDate);
            Assert.Equal(gameEditModel.Developer, secondUpdatedGame.Developer);
            Assert.Equal(gameEditModel.Publisher, secondUpdatedGame.Publisher);
            Assert.Equal(gameEditModel.Price, secondUpdatedGame.Price);
            Assert.Contains("updated_game", secondUpdatedGame.ImageUrl);
            Assert.NotNull(secondUpdatedGame.Categories);
            Assert.Equal(1, secondUpdatedGame.Categories.Count);
            Assert.Contains(secondUpdatedGame.Categories, c => c.Name == "Adventure");

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
        [Fact]
        public async Task GameToGameViewModel_NullData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            var exception = Assert.Throws<MyExceptions.ValidationException>(() => service.GameToGameViewModel(null));

            Assert.Contains("Game cannot be null", exception.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetSomeGames Tests
        [Fact]
        public async Task GetSomeGamesAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            db.Games.AddRange(
                new Game
                {
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            await db.SaveChangesAsync();

            var games = await service.GetSomeGamesAsync(2);

            Assert.NotNull(games);
            Assert.Equal(2, games.Count());
            Assert.NotNull(games[0]);

            DbContext.Dispose(db);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999)]
        public async Task GetSomeGamesAsync_InvalidData(int count)
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            db.Games.AddRange(
                new Game
                {
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            await db.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetSomeGamesAsync(count));

            Assert.Contains($"Count cant be {count}, it must be > 0 and <= 100", exception.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetSimilarGamesAsync Tests
        [Fact]
        public async Task GetSimilarGamesAsync_ValidData_Use_CreateGameAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            db.Categories.AddRange(
                new Category { Id = 1, Name = "Action" },
                new Category { Id = 2, Name = "Adventure" }
            );
            await db.SaveChangesAsync();

            var viewModels = new CreateGameViewModel[]
             {
                new CreateGameViewModel
                {
                    Title = "Action Adventure Game",
                    Description = "Test Description",
                    ReleaseDate = DateTime.Now,
                    Developer = "Test Developer",
                    Publisher = "Test Publisher",
                    Price = 300,
                    SelectedCategoryIds = new List<int> { 1, 2 }
                },
                new CreateGameViewModel
                 {
                     Title = "Action Game",
                     Description = "Test Description",
                     ReleaseDate = DateTime.Now,
                     Developer = "Test Developer",
                     Publisher = "Test Publisher",
                     Price = 300,
                     SelectedCategoryIds = new List<int> { 1 }
                 },
                new CreateGameViewModel
                 {
                     Title = "Adventure Game",
                     Description = "Test Description",
                     ReleaseDate = DateTime.Now,
                     Developer = "Test Developer",
                     Publisher = "Test Publisher",
                     Price = 300,
                     SelectedCategoryIds = new List<int> { 2 }
                 },
                new CreateGameViewModel
                 {
                     Title = "Different Game",
                     Description = "Test Description",
                     ReleaseDate = DateTime.Now,
                     Developer = "Test Developer",
                     Publisher = "Test Publisher",
                     Price = 300,
                     SelectedCategoryIds = new List<int> { }
                 }
             };
            foreach (var vm in viewModels)
            {
                await service.CreateGameAsync(vm);
            }
            db.SaveChanges();

            var gameId = db.Games.First(g => g.Title == "Action Adventure Game").Id;
            var similarGames = await service.GetSimilarGamesAsync(gameId.ToString(), 2);

            Assert.NotNull(similarGames);
            Assert.Equal(2, similarGames.Count());
            Assert.Contains(similarGames, g => g.Title == "Action Game");
            Assert.Contains(similarGames, g => g.Title == "Adventure Game");


            DbContext.Dispose(db);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999)]
        public async Task GetSimilarGamesAsync_InvalidData(int count)
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var service = new GameService(db, _mockEnv.Object, _mockLogger.Object);

            db.Games.AddRange(
                new Game
                {
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            await db.SaveChangesAsync();
            var gameId = db.Games.First(g => g.Title == "Game 1").Id;

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetSimilarGamesAsync(gameId.ToString(), count));

            Assert.Contains($"Count cant be {count}, it must be > 0 and <= 100", exception.Message);

            DbContext.Dispose(db);
        }
        #endregion
    }
}