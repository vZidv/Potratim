using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace src.ViewModel
{
    public class CategoriesIndexViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
    }
}