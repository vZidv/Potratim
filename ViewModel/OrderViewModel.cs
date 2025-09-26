using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class OrderViewModel
    {
        public string? Email { get; set; }
        public CartViewModel Cart { get; set; }
        public IEnumerable<GameViewModel> SameGames { get; set; }
    }
}