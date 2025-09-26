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
using Potratim.ViewModel;

namespace Potratim.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ICartService _cartService;

        public OrderController(UserManager<User> userManager, ICartService cartService)
        {
            _userManager = userManager;
            _cartService = cartService;
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
                Cart = cartViewModel
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}