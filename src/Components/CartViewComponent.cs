using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Potratim.ViewModel;
using Potratim.Services;
using Potratim.Models;

namespace Potratim.Components
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;

        public CartViewComponent(ICartService cartService, UserManager<User> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    var cartItems = await _cartService.GetCartItemsAsync(user.Id);
                    var total = await _cartService.GetCartTotalAsync(user.Id);

                    var viewModel = new CartViewModel()
                    {
                        Items = cartItems,
                        TotalPrice = total,
                        ItemCount = cartItems.Count
                    };
                    return View(viewModel);
                }
            }
            else
            {
                var cartItems = await _cartService.GetCartItemsAsync(HttpContext);
                var total = await _cartService.GetCartTotalAsync(HttpContext);

                var viewModel = new CartViewModel()
                {
                    Items = cartItems,
                    TotalPrice = total,
                    ItemCount = cartItems.Count
                };
                return View(viewModel);
            }

            return View(new CartViewModel());
        }
    }
}