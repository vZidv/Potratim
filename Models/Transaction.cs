using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid GameId { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        public Guid? UserId { get; set; }

        [Required]
        [Column(TypeName = "money")]
        public decimal Cost { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}