using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class GameViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        public DateTime ReleaseDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } = null;

        public List<Category>? Categories { get; set; }

    }
}