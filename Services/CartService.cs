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

namespace Potratim.Services
{
    public class CartService : ICartService
    {
        private readonly PotratimDbContext _context;
        private readonly UserManager<User> _userManager;

        public CartService(PotratimDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddToCartAsync(Guid userId, Guid gameId, int quantity = 1)
        {
            var cart = await GetOrCreateDbCartAsync(userId);

            var cartGame = cart.Games.FirstOrDefault(g => g.Id == gameId);
            if (cartGame == null)
            {
                var game = await _context.Games.FindAsync(gameId);
                cart.Games.Add(game);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid gameId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);
            var cartGame = cart.Games.FirstOrDefault(cg => cg.Id == gameId);
            if (cartGame != null)
            {
                cart.Games.Remove(cartGame);
                await _context.SaveChangesAsync();
            }
        }
        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);
            cart.Games.Clear();
            await _context.SaveChangesAsync();
        }

        public async Task<List<Game>> GetCartItemsAsync(Guid userId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);
            return cart.Games.ToList();
        }

        public async Task<decimal> GetCartTotalAsync(Guid userId)
        {
            var cart = await GetOrCreateDbCartAsync(userId);
            return cart.Games.Sum(g => g.Price);
        }
        


        private async Task<Cart> GetOrCreateDbCartAsync(Guid userId)
        {
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
    }
}