using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class GameIndexViewModel
    {
        public Game Game { get; set; } = null!;
        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<GameViewModel> SameGames { get; set; } = null!;
        public IEnumerable<ReviewViewModel> Reviews { get; set; } = null!;
        public CreateReviewViewModel? CreateReviewModel { get; set; }
        public UserViewModel? CurrentUser { get; set; }
    }
}