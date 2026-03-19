using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Potratim.Data;
using Potratim.Models;
using Potratim.Services;
using src.Services;
using Xunit;

namespace Potratim.Tests.Services
{
    public class CartServiceTests : IDisposable
    {
        private readonly Mock<ILogger<CartService>> _loggerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private string _tempPath;

        public CartServiceTests()
        {
            _loggerMock = new Mock<ILogger<CartService>>();
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempPath))
            {
                Directory.Delete(_tempPath, true);
            }
        }

        private GameService CreateGameService(PotratimDbContext dbContext)
        {
            var gameServiceLoggerMock = new Mock<ILogger<GameService>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempPath);
            mockEnvironment.Setup(e => e.WebRootPath).Returns(_tempPath);
            return new GameService(dbContext, mockEnvironment.Object, gameServiceLoggerMock.Object);
        }
        #region AddToCartAsync Tests for Authorized Users
        [Fact]
        public async Task AddToCartAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task AddToCartAsync_InvalidUserData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            Guid userInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userInvalidId, gameId));

            Assert.Equal($"Invalid user ID {userInvalidId}", exceptions.Message);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task AddToCartAsync_InvalidGameData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            Guid gameInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userId, gameInvalidId));

            Assert.Equal($"Invalid game ID {gameInvalidId}", exceptions.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task AddToCartAsync_UnExistentGame()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            var wrongGameId = Guid.NewGuid();
            var exceptions = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => cartService.AddToCartAsync(userId, wrongGameId));

            Assert.Equal($"Game with ID {wrongGameId} not found", exceptions.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task AddToCartAsync_ValidData_SecondInsert()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId);
            await cartService.AddToCartAsync(userId, gameId);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            DbContext.Dispose(db);
        }
        #endregion

        #region RemoveFromCartAsync Tests
        [Fact]
        public async Task RemoveFromCartAsync_ValidData_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, gameId);
            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            await cartService.RemoveFromCartAsync(userId, gameId);
            cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Equal(cart.Games.Count, 0);


            DbContext.Dispose(db);
        }

        [Fact]
        public async Task RemoveFromCartAsync_InvalidUserData_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, gameId);
            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Single(cart.Games);

            Guid userInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.RemoveFromCartAsync(userInvalidId, gameId));

            Assert.Equal($"Invalid user ID {userInvalidId}", exceptions.Message);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task RemoveFromCartAsync_InvalidGameData_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, gameId);
            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Single(cart.Games);

            Guid gameInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.RemoveFromCartAsync(userId, gameInvalidId));

            Assert.Equal($"Invalid game ID {gameInvalidId}", exceptions.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task RemoveFromCartAsync_UnExistentGame_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, gameId);
            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Single(cart.Games);

            var wrongGameId = Guid.NewGuid();
            var exceptions = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => cartService.RemoveFromCartAsync(userId, wrongGameId));

            Assert.Equal($"Game with ID {wrongGameId} not found in cart", exceptions.Message);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task RemoveFromCartAsync_ValidData_SecondRemove_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, gameId);
            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            await cartService.RemoveFromCartAsync(userId, gameId);
            cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();
            Assert.NotNull(cart);
            Assert.Equal(cart.Games.Count, 0);

            var exception = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => cartService.RemoveFromCartAsync(userId, gameId));
            Assert.Equal($"Game with ID {gameId} not found in cart", exception.Message);


            DbContext.Dispose(db);
        }
        #endregion

        #region ClearCartAsync Tests
        [Fact]
        public async Task ClearCartAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            await cartService.ClearCartAsync(userId);

            cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Equal(cart.Games.Count, 0);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task ClearCartAsync_InvalidUserId()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(async () => await cartService.ClearCartAsync(Guid.Parse("00000000-0000-0000-0000-000000000000")));

            Assert.Equal($"Invalid user ID 00000000-0000-0000-0000-000000000000", exception.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetCartItemsAsync for Authenticated Users Tests
        [Fact]
        public async Task GetCartItemsAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            var cartItems = await cartService.GetCartItemsAsync(userId);

            Assert.NotNull(cartItems);
            Assert.Single(cartItems);
            Assert.Equal(gameId, cartItems.First().Id);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task GetCartItemsAsync_InvalidUserId()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId, cart.Games.First().Id);

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(async () =>
                await cartService.GetCartItemsAsync(Guid.Parse("00000000-0000-0000-0000-000000000000")));

            Assert.Equal($"Invalid user ID 00000000-0000-0000-0000-000000000000", exception.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetCartTotalAsync Tests
        [Fact]
        public async Task GetCartTotalAsync_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId1,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = gameId2,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId1);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId1, cart.Games.First().Id);

            var total = await cartService.GetCartTotalAsync(userId);

            Assert.Equal(100, total);

            await cartService.AddToCartAsync(userId, gameId2);
            total = await cartService.GetCartTotalAsync(userId);

            Assert.Equal(300, total);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task GetCartTotalAsync_InvalidUserId()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId1,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = gameId2,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            });

            await db.SaveChangesAsync();
            await cartService.AddToCartAsync(userId, gameId1);

            var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.Single(cart.Games);
            Assert.Equal(gameId1, cart.Games.First().Id);

            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(async () => await cartService.GetCartTotalAsync(Guid.Parse("00000000-0000-0000-0000-000000000000")));

            Assert.Equal($"Invalid user ID 00000000-0000-0000-0000-000000000000", exception.Message);
            DbContext.Dispose(db);
        }
        #endregion

        #region AddToCartAsync Tests for Unauthorized Users
        [Fact]
        public async Task AddToCartAsync_UnauthorizedUser_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var gameId = Guid.NewGuid();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, gameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Single(cart);
            Assert.Equal(1, cart[gameId]);
            Assert.Equal(gameId, cart.Keys.First());

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task AddToCartAsync_IncrementQuantity_UnauthorizedUser_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var gameId = Guid.NewGuid();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, gameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Single(cart);
            Assert.Equal(1, cart[gameId]);
            Assert.Equal(gameId, cart.Keys.First());

            await cartService.AddToCartAsync(context, gameId);
            cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Single(cart);
            Assert.Equal(2, cart[gameId]);
            Assert.Equal(gameId, cart.Keys.First());

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task AddToCartAsync_UnauthorizedUser_InvalidUserData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            db.Games.AddRange(
                new Game
                {
                    Id = gameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                }
            );

            await db.SaveChangesAsync();
            Guid userInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userInvalidId, gameId));

            Assert.Equal($"Invalid user ID {userInvalidId}", exceptions.Message);

            DbContext.Dispose(db);
        }

        [Fact]
        public async Task AddToCartAsync_UnauthorizedUser_InvalidGameData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var gameId = Guid.Parse("00000000-0000-0000-0000-000000000000");

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(context, gameId));

            Assert.Equal($"Invalid game ID {gameId}", exceptions.Message);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetCartItemsAsync for UnAuthenticated Users Tests

        [Fact]
        public async Task GetCartItemsAsync_UnauthorizedUser_ValidData_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, firstGameId);
            await cartService.AddToCartAsync(context, middleGameId);
            await cartService.AddToCartAsync(context, lastGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(3, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            var cartItems = await cartService.GetCartItemsAsync(context);

            Assert.NotNull(cartItems);
            Assert.Equal(3, cartItems.Count);
            Assert.Equal(firstGameId, cartItems.First().Id);
            Assert.Equal(lastGameId, cartItems.Last().Id);
            Assert.Equal(typeof(Game), cartItems.First().GetType());
            Assert.Equal(games.First(g => g.Id == firstGameId).Price, cartItems.First().Price);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task GetCartItemsAsync_UnauthorizedUser_CartEmpty()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);


            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var cartItems = await cartService.GetCartItemsAsync(context);

            Assert.NotNull(cartItems);
            Assert.Equal(0, cartItems.Count);

            DbContext.Dispose(db);
        }
        #endregion
        #region RemoveFromCartAsync for UnAuthenticated Users Tests
        [Fact]
        public async Task RemoveFromCartAsync_UnauthorizedUser_ValidData_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, firstGameId);
            await cartService.AddToCartAsync(context, middleGameId);
            await cartService.AddToCartAsync(context, lastGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(3, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            await cartService.RemoveFromCartAsync(context, middleGameId);
            cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(2, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task RemoveFromCartAsync_UnauthorizedUser_UnExistingGame_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, firstGameId);
            await cartService.AddToCartAsync(context, middleGameId);
            await cartService.AddToCartAsync(context, lastGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(3, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            await cartService.RemoveFromCartAsync(context, Guid.NewGuid());
            cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(3, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            DbContext.Dispose(db);
        }
        #endregion
        #region ClearCartAsync for UnAuthenticated Users Tests
        [Fact]
        public async Task ClearCartAsync_UnauthorizedUser_ValidData_Use_AddToCart()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, firstGameId);
            await cartService.AddToCartAsync(context, middleGameId);
            await cartService.AddToCartAsync(context, lastGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(3, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            await cartService.ClearCartAsync(context);
            cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(0, cart.Count);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task ClearCartAsync_UnauthorizedUser_EmptyCart()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(0, cart.Count);

            await cartService.ClearCartAsync(context);
            cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(0, cart.Count);

            DbContext.Dispose(db);
        }
        #endregion

        #region GetCartTotalAsync for UnAuthenticated Users Tests
        [Fact]
        public async Task GetCartTotalAsync_UnauthorizedUser_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, firstGameId);
            await cartService.AddToCartAsync(context, middleGameId);
            await cartService.AddToCartAsync(context, lastGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(3, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());
            Assert.Equal(lastGameId, cart.Keys.Last());

            var total = await cartService.GetCartTotalAsync(context);
            Assert.Equal(600, total);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task GetCartTotalAsync_UnauthorizedUser_With_Empty_Cart()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var total = await cartService.GetCartTotalAsync(context);
            Assert.Equal(0, total);

            DbContext.Dispose(db);
        }
        #endregion

        #region Other Tests
        [Fact]
        public async Task GetSessionCart_UnauthorizedUser_With_Empty_HttpContext()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            HttpContext context = null;
            var exception = await Assert.ThrowsAsync<MyExceptions.ValidationException>(async () => cartService.GetSessionCart(context));

            Assert.Equal("HttpContext cannot be null", exception.Message);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task MergeCartsAsync_ValidData_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            await cartService.AddToCartAsync(context, firstGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(1, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());

            var userId = Guid.NewGuid();
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "user@example.com"
            });
            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, middleGameId);
            await cartService.AddToCartAsync(userId, lastGameId);

            var userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(2, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            await cartService.MergeCartAsync(context, userId);
            userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(3, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == firstGameId);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task MergeCartsAsync_EmptySessionCart_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var userId = Guid.NewGuid();
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "user@example.com"
            });
            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, middleGameId);
            await cartService.AddToCartAsync(userId, lastGameId);

            var userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(2, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            await cartService.MergeCartAsync(context, userId);
            userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(2, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task MergeCartsAsync_EmptySessionCart_With_UnExistingUser_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var userId = Guid.NewGuid();
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "user@example.com"
            });
            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, middleGameId);
            await cartService.AddToCartAsync(userId, lastGameId);

            var userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(2, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            await cartService.MergeCartAsync(context, Guid.NewGuid());
            userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(2, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            DbContext.Dispose(db);
        }
        [Fact]
        public async Task MergeCartsAsync_With_UnExistingGame_Use_AddToCartAsync()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var firstGameId = Guid.NewGuid();
            var middleGameId = Guid.NewGuid();
            var lastGameId = Guid.NewGuid();

            var games = new List<Game>
            {
                new Game
                {
                    Id = firstGameId,
                    Title = "Game 1",
                    Description = "Description 1",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 1",
                    Publisher = "Publisher 1",
                    Price = 100
                },
                new Game
                {
                    Id = middleGameId,
                    Title = "Game 2",
                    Description = "Description 2",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 2",
                    Publisher = "Publisher 2",
                    Price = 200
                },
                new Game
                {
                    Id = lastGameId,
                    Title = "Game 3",
                    Description = "Description 3",
                    ReleaseDate = DateTime.Now,
                    Developer = "Developer 3",
                    Publisher = "Publisher 3",
                    Price = 300
                }
            };
            db.Games.AddRange(games);
            await db.SaveChangesAsync();

            var context = new DefaultHttpContext();
            context.Features.Set<ISessionFeature>(new SessionFeature { Session = new TestSession() });
            await context.Session.LoadAsync();

            var unExistingGameId = Guid.NewGuid();
            await cartService.AddToCartAsync(context, firstGameId);
            await cartService.AddToCartAsync(context, unExistingGameId);
            var cart = cartService.GetSessionCart(context);

            Assert.NotNull(cart);
            Assert.Equal(2, cart.Count);
            Assert.Equal(firstGameId, cart.Keys.First());

            var userId = Guid.NewGuid();
            db.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "user@example.com"
            });
            await db.SaveChangesAsync();

            await cartService.AddToCartAsync(userId, middleGameId);
            await cartService.AddToCartAsync(userId, lastGameId);

            var userCart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

            Assert.NotNull(userCart);
            Assert.Equal(2, userCart.Games.Count);
            Assert.Contains(userCart.Games, g => g.Id == middleGameId);
            Assert.Contains(userCart.Games, g => g.Id == lastGameId);

            var exception = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => cartService.MergeCartAsync(context, userId));

            Assert.Equal($"Game with ID {unExistingGameId} not found", exception.Message);
            DbContext.Dispose(db);
        }
        #endregion
    }
}