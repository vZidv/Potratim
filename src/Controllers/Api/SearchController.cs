using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;
using Potratim.ViewModel;

namespace Potratim.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly PotratimDbContext _context;

        public SearchController(PotratimDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new { games = new List<GameViewModel>() });
            }

            var games = await _context.Games
            .Where(g => EF.Functions.ILike(g.Title, $"%{query}%"))
            .Take(10)
            .ToListAsync();

            List<GameViewModel> gameViewModels = games.Select(g => new GameViewModel
            {
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                ImageUrl = g.ImageUrl
            }).ToList();

            return Ok(new { games = gameViewModels });
        }
    }
}