using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Potratim.ViewModel
{
    public class HomeIndexViewModel
    {
        [Display(Name = "Новинки")]
        public List<GameViewModel> NewGames { get; set; } = new List<GameViewModel>();
    }
}