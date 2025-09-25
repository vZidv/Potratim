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
        Task AddToCartAsync(Guid userId, Guid gameId, int quantity = 1);
        Task RemoveFromCartAsync(Guid userId, Guid gameId);
        Task ClearCartAsync(Guid userId);
        Task<decimal> GetCartTotalAsync(Guid userId);
        //Task MergeCartAsync(HttpContext httpContext, Guid userId);
    }
}