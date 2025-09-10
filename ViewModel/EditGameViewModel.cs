using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class EditGameViewModel
    {
        public Guid Id { get; set; }

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

        public string? CurrentImageUrl { get; set; } = null;

        [Display(Name = "Новая обложка игры")]
        public IFormFile? ImageFile { get; set; } = null;
    }
}