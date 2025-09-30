using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;
using Potratim.ViewModel;

namespace Potratim.Services
{
    public interface ICartService
    {
        Task<List<Game>> GetCartItemsAsync(Guid userId);
        Task<List<Game>> GetCartItemsAsync(HttpContext httpContext);

        Task AddToCartAsync(Guid userId, Guid gameId, int quantity = 1);
        Task AddToCartAsync(HttpContext httpContext, Guid gameId, int quantity = 1);

        Task RemoveFromCartAsync(Guid userId, Guid gameId);
        Task RemoveFromCartAsync(HttpContext httpContext, Guid gameId);

        Task ClearCartAsync(Guid userId);
        Task ClearCartAsync(HttpContext httpContext);
        
        Task<decimal> GetCartTotalAsync(Guid userId);
        Task<decimal> GetCartTotalAsync(HttpContext httpContext);

        Task MergeCartAsync(HttpContext httpContext, Guid userId);
    }
}