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

        public CartController(ICartService cartService, UserManager<User> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IActionResult> AddToCartAsync(string gameId)
        {
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
                return View("Error", ex);
            }
            return RedirectToAction("Index", "Game", new { id = gameId });

        }
        public async Task<IActionResult> RemoveFromCartAsync(string gameId)
        {
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