using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Potratim.Models
{
    public class Game
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        [Column(TypeName = "date")]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Developer { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Publisher { get; set; } = null!;

        [Required]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } = null;


        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Cart> Carts { get; set; } = new List<Cart>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}