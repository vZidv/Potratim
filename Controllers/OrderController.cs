using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.Services;
using Potratim.ViewModel;

namespace Potratim.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ICartService _cartService;
        private readonly PotratimDbContext _context;

        public OrderController(UserManager<User> userManager, ICartService cartService, PotratimDbContext context)
        {
            _userManager = userManager;
            _cartService = cartService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userEmail = await _userManager.GetEmailAsync(user);

            var cartViewModel = new CartViewModel()
            {
                Items = await _cartService.GetCartItemsAsync(user.Id),
                TotalPrice = await _cartService.GetCartTotalAsync(user.Id),
                ItemCount = (await _cartService.GetCartItemsAsync(user.Id)).Count()
            };

            var model = new OrderViewModel()
            {
                Email = userEmail,
                Cart = cartViewModel,
                SameGames = await GetSomeGamesAsync(10)
            };
            return View(model);
        }

        public IActionResult OrderFinished()
        {
            return View();
        }

        public async Task<List<GameViewModel>> GetSomeGamesAsync(int count)
        {
            var someGames = await _context.Games.Take(count).ToListAsync();

            return someGames.Select(g => new GameViewModel()
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl,
                Categories = g.Categories.ToList()
            }).ToList();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}