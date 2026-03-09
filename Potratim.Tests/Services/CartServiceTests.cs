using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        #region AddToCartAsync Tests
        // [Fact]
        // public async Task AddToCartAsync_UnauthorizedUser_ValidData()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var gameId = Guid.NewGuid();

        //     var sessionMock = new Mock<ISession>();
        //     var sessionData = new Dictionary<Guid, int> { { Guid.NewGuid(), 2 } };
        //     var serialized = JsonSerializer.Serialize(sessionData);
        //     var bytes = System.Text.Encoding.UTF8.GetBytes(serialized);

        //     sessionMock.Setup(s => s.TryGetValue("Cart", out bytes)).Returns(true);

        //     var httpContextMock = new Mock<HttpContext>();
        //     httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
        //     httpContextMock.Setup(c => c.User.Identity.Name).Returns("testuser");

        //     await cartService.AddToCartAsync(httpContextMock.Object, gameId);
        //     var cart = cartService.GetSessionCart(httpContextMock.Object);

        //     Assert.NotNull(cart);
        //     Assert.Single(cart);

        //     DbContext.Dispose(db);
        // }
        [Fact]
        public async Task AddToCartAsync_IncrementQuantity_UnauthorizedUser_ValidData()
        {
            using var db = DbContext.CreateInMemoryDbContext();
            var gameService = CreateGameService(db);
            var cartService = new CartService(db, gameService, _loggerMock.Object);

            var gameId = Guid.NewGuid();

            var mockHttpContext = new Mock<HttpContext>();
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue("Cart", out It.Ref<byte[]>.IsAny)).Returns(false);
        

            await cartService.AddToCartAsync(mockHttpContext.Object, gameId);
            var cart = cartService.GetSessionCart(mockHttpContext.Object);

            Assert.NotNull(cart);
            Assert.Single(cart);

            await cartService.AddToCartAsync(mockHttpContext.Object, gameId);
            cart = cartService.GetSessionCart(mockHttpContext.Object);

            Assert.NotNull(cart);
            Assert.Single(cart);
            Assert.Equal(3, cart[gameId]);

            DbContext.Dispose(db);
        }

        // [Fact]
        // public async Task AddToCartAsync_UnauthorizedUser_InvalidUserData()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );

        //     await db.SaveChangesAsync();
        //     Guid userInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        //     var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userInvalidId, gameId));

        //     Assert.Equal($"Invalid user ID {userInvalidId}", exceptions.Message);

        //     DbContext.Dispose(db);
        // }
        // [Fact]
        // public async Task AddToCartAsync_InvalidGameData()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     Guid gameInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        //     var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userId, gameInvalidId));

        //     Assert.Equal($"Invalid game ID {gameInvalidId}", exceptions.Message);

        //     DbContext.Dispose(db);
        // }

        // [Fact]
        // public async Task AddToCartAsync_UnExistentGame()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     var wrongGameId = Guid.NewGuid();
        //     var exceptions = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => cartService.AddToCartAsync(userId, wrongGameId));

        //     Assert.Equal($"Game with ID {wrongGameId} not found", exceptions.Message);

        //     DbContext.Dispose(db);
        // }

        // [Fact]
        // public async Task AddToCartAsync_ValidData_SecondInsert()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     await cartService.AddToCartAsync(userId, gameId);
        //     await cartService.AddToCartAsync(userId, gameId);

        //     var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

        //     Assert.NotNull(cart);
        //     Assert.Single(cart.Games);
        //     Assert.Equal(gameId, cart.Games.First().Id);

        //     DbContext.Dispose(db);
        // }

        // [Fact]
        // public async Task AddToCartAsync_ValidData()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     await cartService.AddToCartAsync(userId, gameId);

        //     var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

        //     Assert.NotNull(cart);
        //     Assert.Single(cart.Games);
        //     Assert.Equal(gameId, cart.Games.First().Id);

        //     DbContext.Dispose(db);
        // }

        // [Fact]
        // public async Task AddToCartAsync_InvalidUserData()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     Guid userInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        //     var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userInvalidId, gameId));

        //     Assert.Equal($"Invalid user ID {userInvalidId}", exceptions.Message);

        //     DbContext.Dispose(db);
        // }
        // [Fact]
        // public async Task AddToCartAsync_InvalidGameData()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     Guid gameInvalidId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        //     var exceptions = await Assert.ThrowsAsync<MyExceptions.ValidationException>(() => cartService.AddToCartAsync(userId, gameInvalidId));

        //     Assert.Equal($"Invalid game ID {gameInvalidId}", exceptions.Message);

        //     DbContext.Dispose(db);
        // }

        // [Fact]
        // public async Task AddToCartAsync_UnExistentGame()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     var wrongGameId = Guid.NewGuid();
        //     var exceptions = await Assert.ThrowsAsync<MyExceptions.GameNotFoundException>(() => cartService.AddToCartAsync(userId, wrongGameId));

        //     Assert.Equal($"Game with ID {wrongGameId} not found", exceptions.Message);

        //     DbContext.Dispose(db);
        // }

        // [Fact]
        // public async Task AddToCartAsync_ValidData_SecondInsert()
        // {
        //     using var db = DbContext.CreateInMemoryDbContext();
        //     var gameService = CreateGameService(db);
        //     var cartService = new CartService(db, gameService, _loggerMock.Object);

        //     var userId = Guid.NewGuid();
        //     var gameId = Guid.NewGuid();

        //     db.Games.AddRange(
        //         new Game
        //         {
        //             Id = gameId,
        //             Title = "Game 1",
        //             Description = "Description 1",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 1",
        //             Publisher = "Publisher 1",
        //             Price = 100
        //         },
        //         new Game
        //         {
        //             Id = Guid.NewGuid(),
        //             Title = "Game 2",
        //             Description = "Description 2",
        //             ReleaseDate = DateTime.Now,
        //             Developer = "Developer 2",
        //             Publisher = "Publisher 2",
        //             Price = 200
        //         }
        //     );
        //     db.Users.Add(new User
        //     {
        //         Id = userId,
        //         UserName = "testuser",
        //         Email = "test@example.com"
        //     });

        //     await db.SaveChangesAsync();
        //     await cartService.AddToCartAsync(userId, gameId);
        //     await cartService.AddToCartAsync(userId, gameId);

        //     var cart = await db.Carts.Where(c => c.UserId == userId).Include(c => c.Games).FirstOrDefaultAsync();

        //     Assert.NotNull(cart);
        //     Assert.Single(cart.Games);
        //     Assert.Equal(gameId, cart.Games.First().Id);

        //     DbContext.Dispose(db);
        // }
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

        #region GetCartItemsAsync Tests
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

    }
}