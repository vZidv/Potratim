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
    }
}