using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class CreateReviewViewModel
    {

        [Required(ErrorMessage = "ID игры является обязательным.")]
        public Guid GameId { get; set; }

        [Required(ErrorMessage = "Оценка, обязательна.")]
        public bool? Like { get; set; } = null;

        [Required(ErrorMessage = "Отзыв не может быть пустым.")]
        [StringLength(2000, ErrorMessage = "Отзыв не может превышать 2000 символов.")]
        public string Comment { get; set; }
    }
}