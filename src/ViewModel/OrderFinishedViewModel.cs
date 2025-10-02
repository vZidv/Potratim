using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class OrderFinishedViewModel
    {
        public string Email { get; set; } = null!;
        public List<Game> Games { get; set; } = new List<Game>();
        public decimal TotalCost { get; set; }
    }
}