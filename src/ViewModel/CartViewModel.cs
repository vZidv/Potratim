using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class CartViewModel
    {
        public List<Game> Items { get; set; } = new List<Game>();
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
    }
}