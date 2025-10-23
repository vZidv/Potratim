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
using src.Services;

namespace Potratim.Controllers
{

    public class OrderController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ICartService _cartService;
        private readonly PotratimDbContext _context;
        private readonly IGameService _gameService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            UserManager<User> userManager,
            ICartService cartService,
            PotratimDbContext context,
            IGameService gameService,
            ILogger<OrderController> logger)
        {
            _userManager = userManager;
            _cartService = cartService;
            _context = context;
            _gameService = gameService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading order page");

            string? userEmail = null;
            CartViewModel? cartViewModel = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                userEmail = await _userManager.GetEmailAsync(user);


                cartViewModel = new CartViewModel()
                {
                    Items = await _cartService.GetCartItemsAsync(user.Id),
                    TotalPrice = await _cartService.GetCartTotalAsync(user.Id),
                    ItemCount = (await _cartService.GetCartItemsAsync(user.Id)).Count()
                };
            }
            else
            {
                var cartItems = await _cartService.GetCartItemsAsync(HttpContext);
                var total = await _cartService.GetCartTotalAsync(HttpContext);

                cartViewModel = new CartViewModel()
                {
                    Items = cartItems,
                    TotalPrice = total,
                    ItemCount = cartItems.Count
                };
            }

            var model = new OrderViewModel()
            {
                Email = userEmail,
                Cart = cartViewModel,
                SameGames = await _gameService.GetSomeGamesAsync(10)
            };
            return View(model);
        }

        public async Task<IActionResult> OrderFinished(OrderFinishedViewModel model)
        {
            _logger.LogInformation("Finishing order");
            _logger.LogDebug($"Order details: {model}");
            
            User? user = null;
            if (User.Identity.IsAuthenticated)
                user = await _userManager.GetUserAsync(User);

            List<Game> purchasedGames = await (user != null ? _cartService.GetCartItemsAsync(user.Id) : _cartService.GetCartItemsAsync(HttpContext));

            foreach (var item in purchasedGames)
            {
                Transaction transaction = new Transaction()
                {
                    GameId = item.Id,
                    Email = model.Email,
                    UserId = user?.Id,
                    Cost = item.Price,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
            }


            if (user != null)
            {
                foreach (var item in await _cartService.GetCartItemsAsync(user.Id))
                {
                    if (!user.Games.Contains(item))
                        user.Games.Add(item);
                }
                await _cartService.ClearCartAsync(user.Id);
            }
            else
            {
                await _cartService.ClearCartAsync(HttpContext);
            }

            ViewBag.TotalCost = model.TotalCost;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}