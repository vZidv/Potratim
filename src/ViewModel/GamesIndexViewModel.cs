using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;
using X.PagedList;

namespace src.ViewModel
{
    public class GamesIndexViewModel
    {
        public IPagedList<Game> Games { get; set; } = null!;
        //Filters
        public string SearchTerm { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "Id";
        public int? PriceMin { get; set; } = null;
        public int? PriceMax { get; set; } = null;
        public DateTime? ReleaseDateFrom { get; set; } = null;
        public DateTime? ReleaseDateTo { get; set; } = null;
    }
}