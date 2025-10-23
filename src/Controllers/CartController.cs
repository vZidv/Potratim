using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Potratim.Models;
using Potratim.Services;

namespace Potratim.Controllers
{
    //[Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, UserManager<User> userManager, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> AddToCartAsync(string gameId)
        {
            _logger.LogInformation($"Initiating add to cart process for game ID: {gameId}");
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    await _cartService.AddToCartAsync(user.Id, Guid.Parse(gameId));
                }
                else
                {
                    await _cartService.AddToCartAsync(HttpContext, Guid.Parse(gameId));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding game to cart: {gameId}");
                return View("Error", ex);
            }
            return RedirectToAction("Index", "Game", new { id = gameId });

        }
        public async Task<IActionResult> RemoveFromCartAsync(string gameId)
        {
            _logger.LogInformation($"Initiating remove from cart process for game ID: {gameId}");
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                await _cartService.RemoveFromCartAsync(user.Id, Guid.Parse(gameId));
            }
            else
            {
                await _cartService.RemoveFromCartAsync(HttpContext, Guid.Parse(gameId));
            }
            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);
            else
                return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}