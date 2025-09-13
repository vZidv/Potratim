using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;
using X.PagedList;

namespace Potratim.ViewModel
{
    public class CatalogIndexViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public IPagedList<GameViewModel> Games { get; set; }

        //Filters

        public string? SearchString { get; set; } = string.Empty;
        public List<int>? SelectedCategoriesId { get; set; } = new();
        public decimal? MinPrice { get; set; } = 0;
        public decimal? MaxPrice { get; set; } = 15000;
    }
}