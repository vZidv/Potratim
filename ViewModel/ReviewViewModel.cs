using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class ReviewViewModel
    {

        public UserViewModel User { get; set; } = null!;

        public Guid GameId { get; set; }

        public bool? Like { get; set; } = null;

        public string? Comment { get; set; } = null;
    }
}