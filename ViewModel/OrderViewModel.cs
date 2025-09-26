using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class OrderViewModel
    {
        public string? Email { get; set; }
        public CartViewModel Cart { get; set; }
    }
}