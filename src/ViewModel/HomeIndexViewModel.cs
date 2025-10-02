using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class HomeIndexViewModel
    {
        [Display(Name = "Новинки")]
        public List<GameViewModel> NewGames { get; set; } = new List<GameViewModel>();

        public List<GameViewModel> SomeGames { get; set; } = new List<GameViewModel>();

        public List<List<GameViewModel>> CategoriesGames { get; set; } = new List<List<GameViewModel>>();

        public List<Category> Categories { get; set; } = new List<Category>();
    }
} 