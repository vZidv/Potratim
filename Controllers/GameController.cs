using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Potratim.Data;
using Potratim.Models;

namespace Potratim.Controllers
{
    [Route("[controller]")]
    public class GameController : Controller
    {
        private readonly ILogger<GameController> _logger;
        private readonly PotratimDbContext _context;

        private Game _game;

        public GameController(ILogger<GameController> logger, PotratimDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string? gameTitle = "The Witcher 3: Wild Hunt")
        {
            _game = _context.Games.Where(g => g.Title == gameTitle).FirstOrDefault();
            ViewBag.Game = _game;

            return View();
        }
    }
}