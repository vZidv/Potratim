using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging;
using Moq;
using Potratim.Data;
using Potratim.Models;
using Potratim.MyExceptions;
using src.Services;
using Xunit;

namespace Potratim.Tests.Services
{
    public class GameServiceTests
    {
        private PotratimDbContext _mockContext;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<GameService>> _mockLogger;

        public GameServiceTests()
        {
            _mockContext = CreateInMemoryDbContext();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<GameService>>();
        }

        private PotratimDbContext CreateInMemoryDbContext(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<PotratimDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;
            return new PotratimDbContext(options);
        }

        #region GetGameAsync(string id) Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetGameAsync_NullOrWhitespaceId(string id)
        {
            var service = new GameService(_mockContext, _mockEnv.Object, _mockLogger.Object);

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetGameAsync(id));

            Assert.Equal(nameof(id), exception.PropertyName);
        }
        //         [Fact]
        //         public async Task GetGameAsync_InvalidGameId()
        //         {
        //             var mockContext = GetMockDbContext();
        //             var mockEnv = GetMockEnvironment();
        //             var mockLogger = GetMockLogger();
        //             var service = new GameService(mockContext.Object, mockEnv.Object, mockLogger.Object);
        //             string invalidId = "invalid-guid";

        //             var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => service.GetGameAsync(invalidId));

        //             Assert.Equal(nameof(invalidId), exception.PropertyName);
        //             Assert.Contains(nameof(invalidId), exception.PropertyName);
        //         }

        //         [Fact]
        //         public async Task GetGameAsync_GameNotFound()
        //         {
        //             var gameId = Guid.NewGuid();
        //             var mockContext = GetMockDbContext();
        //             var mockEnv = GetMockEnvironment();
        //             var mockLogger = GetMockLogger();

        //             var gameData = new List<Game>().AsQueryable();
        //             var mockGameDbSet = new Mock<DbSet<Game>>();
        //             mockGameDbSet.As<IQueryable<Game>>().Setup(m => m.Provider).Returns(gameData.Provider);
        //             mockGameDbSet.As<IQueryable<Game>>().Setup(m => m.Expression).Returns(gameData.Expression);
        //             mockGameDbSet.As<IQueryable<Game>>().Setup(m => m.ElementType).Returns(gameData.ElementType);
        //             mockGameDbSet.As<IQueryable<Game>>().Setup(m => m.GetEnumerator()).Returns(gameData.GetEnumerator());

        //             mockContext.Setup(m => m.Games).Returns(mockGameDbSet.Object);

        //             var service = new GameService(mockContext.Object, mockEnv.Object, mockLogger.Object);

        //             var exception = await Assert.ThrowsAsync<GameNotFoundException>(
        //     () => service.GetGameAsync(gameId.ToString())
        // );

        //             Assert.Equal(gameId, exception.GameId);
        //         }

        //         [Fact]
        //         public async Task GetGameAsync_ValidId_ReturnsGame()
        //         {
        //             // arrange
        //             var gameId = Guid.NewGuid();
        //             var game = new Game
        //             {
        //                 Id = gameId,
        //                 Title = "Test Game",
        //                 Description = "Test Description",
        //                 Price = 29.99m
        //             };

        //             var mockContext = GetMockDbContext();
        //             var mockEnv = GetMockEnvironment();
        //             var mockLogger = GetMockLogger();

        //             // mocируем DbSet для Games с нужной игрой
        //             var gamesData = new List<Game> { game }.AsQueryable();
        //             var mockGamesDbSet = new Mock<DbSet<Game>>();
        //             mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.Provider).Returns(gamesData.Provider);
        //             mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.Expression).Returns(gamesData.Expression);
        //             mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.ElementType).Returns(gamesData.ElementType);
        //             mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.GetEnumerator()).Returns(gamesData.GetEnumerator());

        //             mockContext.Setup(c => c.Games).Returns(mockGamesDbSet.Object);

        //             var service = new GameService(mockContext.Object, mockEnv.Object, mockLogger.Object);


        //             var result = await service.GetGameAsync(gameId.ToString());


        //             Assert.NotNull(result);
        //             Assert.Equal(gameId, result.Id);
        //             Assert.Equal("Test Game", result.Title);
        //         }
        #endregion


        // #region GetGameAsync(Guid id) Tests 

        // [Fact]
        // public async Task GetGameAsync_WithGuid_CallsStringOverload()
        // {
        //     // arrange
        //     var gameId = Guid.NewGuid();
        //     var game = new Game
        //     {
        //         Id = gameId,
        //         Title = "Test Game"
        //     };

        //     var mockContext = GetMockDbContext();
        //     var mockEnv = GetMockEnvironment();
        //     var mockLogger = GetMockLogger();

        //     var gamesData = new List<Game> { game }.AsQueryable();
        //     var mockGamesDbSet = new Mock<DbSet<Game>>();
        //     mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.Provider).Returns(gamesData.Provider);
        //     mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.Expression).Returns(gamesData.Expression);
        //     mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.ElementType).Returns(gamesData.ElementType);
        //     mockGamesDbSet.As<IQueryable<Game>>().Setup(m => m.GetEnumerator()).Returns(gamesData.GetEnumerator());

        //     mockContext.Setup(c => c.Games).Returns(mockGamesDbSet.Object);

        //     var service = new GameService(mockContext.Object, mockEnv.Object, mockLogger.Object);

        //     // act
        //     var result = await service.GetGameAsync(gameId);

        //     // assert
        //     Assert.NotNull(result);
        //     Assert.Equal(gameId, result.Id);
        // }
        // #endregion
    }
}