using System.ComponentModel.DataAnnotations;

namespace Potratim.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public bool? Like { get; set; } = null;

        public string? Comment { get; set; } = null;

        public User User { get; set; } = null!;
        public Game Game { get; set; } = null!;
    }
}