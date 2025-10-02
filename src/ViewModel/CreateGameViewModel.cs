using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class CreateGameViewModel
    {
        [Required]
        [StringLength(255)]
        [Display(Name = "Название")]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        [Display(Name = "Описание")]
        public string Description { get; set; } = null!;

        [Required]
        [Column(TypeName = "date")]
        [Display(Name = "Дата релиза")]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Разработчик")]
        public string Developer { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Display(Name = "Издатель")]
        public string Publisher { get; set; } = null!;

        [Required]
        [Column(TypeName = "money")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        public IFormFile? ImageFile { get; set; } = null;

        [Display(Name = "Категории")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<Category>? Categories { get; set; }
    }
}