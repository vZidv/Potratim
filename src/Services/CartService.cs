using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using src.Services;
using Potratim.MyExceptions;

namespace Potratim.Services
{
    public class CartService : ICartService
    {
        private readonly PotratimDbContext _context;
        private readonly IGameService _gameService;
        private readonly ILogger<CartService> _logger;

        public CartService(
            PotratimDbContext context,
            IGameService gameService,
            ILogger<CartService> logger)
        {
            _context = context;
            _gameService = gameService;
            _logger = logger;
        }


        //Authorization users
        public async Task AddToCartAsync(Guid userId, Guid gameId, int quantity = 1)
        {
            if (gameId == Guid.Empty)
            {
                throw new ValidationException($"Invalid game ID {gameId}")
                {
                    PropertyName = nameof(gameId),
                    AttemptedValue = gameId
                };
            }

            var cart = await GetOrCreateDbCartAsync(userId);

            _logger.LogInformation($"Adding game {gameId} to cart for user {userId}");

            var cartGame = cart.Games.FirstOrDefault(g => g.Id == gameId);
            if (cartGame == null)
            {
                var game = await _gameService.GetGameAsync(gameId);
                cart.Games.Add(game);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation($"Game {gameId} already exists in cart for user {userId}");
            }
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid gameId)
        {
            if (gameId == Guid.Empty)
            {
                throw new ValidationException($"Invalid game ID {gameId}")
                {
                    PropertyName = nameof(gameId),
                    AttemptedValue = gameId
                };
            }

            var cart = await GetOrCreateDbCartAsync(userId);

            _logger.LogInformation($"Removing game {gameId} from cart for user {userId}");

            var cartGame = cart.Games.FirstOrDefault(cg => cg.Id == gameId);
            if (cartGame != null)
            {
                cart.Games.Remove(cartGame);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning($"Game {gameId} not found in cart for user {userId}");
                throw new GameNotFoundException($"Game with ID {gameId} not found in cart");
            }
        }
        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);

            _logger.LogInformation($"Clearing cart for user {userId}");

            cart.Games.Clear();
            await _context.SaveChangesAsync();
        }

        public async Task<List<Game>> GetCartItemsAsync(Guid userId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);
            _logger.LogDebug($"Retrieving cart items for user {userId}");
            return cart.Games.ToList();
        }

        public async Task<decimal> GetCartTotalAsync(Guid userId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);
            _logger.LogDebug($"Calculating cart total for user {userId}");
            return cart.Games.Sum(g => g.Price);
        }

        private async Task<Cart> GetOrCreateDbCartAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ValidationException($"Invalid user ID {userId}")
                {
                    PropertyName = nameof(userId),
                    AttemptedValue = userId
                };
            }
            _logger.LogDebug($"Retrieving or creating cart for user {userId}");


            var cart = await _context.Carts.Include(c => c.Games)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        //unAuthorized users

        public async Task<List<Game>> GetCartItemsAsync(HttpContext httpContext)
        {
            _logger.LogDebug($"Retrieving cart items for user {httpContext.User.Identity.Name}");

            var sessionCart = GetSessionCart(httpContext);
            if (!sessionCart.Any())
            {
                return new List<Game>();
            }

            var gameIds = sessionCart.Keys.ToList();
            var games = await _context.Games
                .Where(g => gameIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g);

            var items = new List<Game>();
            foreach (var kvp in sessionCart)
            {
                if (games.TryGetValue(kvp.Key, out var game))
                {
                    items.Add(new Game
                    {
                        Id = game.Id,
                        Title = game.Title,
                        ImageUrl = game.ImageUrl,
                        Price = game.Price,
                    });
                }
            }
            return items;
        }

        public async Task AddToCartAsync(HttpContext httpContext, Guid gameId, int quantity = 1)
        {
            var sessionCart = GetSessionCart(httpContext);

            _logger.LogInformation($"Adding game {gameId} to cart for user {httpContext.User.Identity.Name}");

            if (sessionCart.ContainsKey(gameId))
            {
                sessionCart[gameId] += quantity;
            }
            else
            {
                sessionCart[gameId] = quantity;
            }

            SaveSessionCart(httpContext, sessionCart);
        }

        public async Task RemoveFromCartAsync(HttpContext httpContext, Guid gameId)
        {
            _logger.LogInformation($"Removing game {gameId} from cart for user {httpContext.User.Identity.Name}");

            var sessionCart = GetSessionCart(httpContext);
            if (sessionCart.ContainsKey(gameId))
            {
                sessionCart.Remove(gameId);
                SaveSessionCart(httpContext, sessionCart);
            }
        }

        public Task ClearCartAsync(HttpContext httpContext)
        {
            _logger.LogInformation($"Clearing cart for user {httpContext.User.Identity.Name}");

            var sessionCart = GetSessionCart(httpContext);
            sessionCart.Clear();
            SaveSessionCart(httpContext, sessionCart);
            return Task.CompletedTask;
        }

        public async Task<decimal> GetCartTotalAsync(HttpContext httpContext)
        {
            _logger.LogDebug($"Calculating cart total for user {httpContext.User.Identity.Name}");

            var sessionCart = GetSessionCart(httpContext);
            if (!sessionCart.Any())
            {
                return 0;
            }

            var gameIds = sessionCart.Keys.ToList();
            var games = await _context.Games
                .Where(g => gameIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g);

            return games.Sum(g => g.Value.Price * sessionCart[g.Key]);
        }


        public async Task MergeCartAsync(HttpContext httpContext, Guid userId)
        {
            _logger.LogDebug($"Merging cart for user {userId}");

            var sessionCart = GetSessionCart(httpContext);
            if (!sessionCart.Any())
            {
                return;
            }

            var dbCart = await GetOrCreateDbCartAsync(userId);

            foreach (var kvp in sessionCart)
            {
                var gameId = kvp.Key;
                var quantity = kvp.Value;

                var existingItem = dbCart.Games.FirstOrDefault(cg => cg.Id == gameId);
                if (existingItem != null)
                {
                    continue;
                }
                else
                {
                    var gameExists = await _gameService.GetGameAsync(gameId);
                    if (gameExists != null)
                    {
                        dbCart.Games.Add(gameExists);
                    }
                }
            }

            await _context.SaveChangesAsync();
            httpContext.Session.Remove("Cart");
        }

        public Dictionary<Guid, int> GetSessionCart(HttpContext httpContext)
        {
            if (httpContext == null)
            {
               throw new ValidationException($"HttpContext cannot be null")
                {
                    PropertyName = nameof(httpContext),
                    AttemptedValue = null
                };
            }

            _logger.LogDebug($"Retrieving session cart for user {httpContext.User.Identity.Name}");

            var sessionCartJson = httpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionCartJson))
            {
                return new Dictionary<Guid, int>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<Guid, int>>(sessionCartJson) ?? new Dictionary<Guid, int>();
            }
            catch
            {
                return new Dictionary<Guid, int>();
            }
        }

        private void SaveSessionCart(HttpContext httpContext, Dictionary<Guid, int> cart)
        {
            _logger.LogDebug($"Saving session cart for user {httpContext.User.Identity.Name}");

            var sessionCartJson = JsonSerializer.Serialize(cart);
            httpContext.Session.SetString("Cart", sessionCartJson);
        }
    }
}