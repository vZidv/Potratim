using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Potratim.Models
{
    public class User : IdentityUser<Guid>
    {
        // [Key]
        // public Guid Id { get; set; }

        [StringLength(40)]
        public string? Nickname { get; set; } = null;

        // [Required]
        // [StringLength(50)]
        // public string Email { get; set; } = null!;

        // [StringLength(255)]
        // public string? PasswordHash { get; set; } = null;

        [Required]
        [Column(TypeName = "money")]
        public decimal Balance { get; set; } = 0;

        public string? ProfileImageUrl { get; set; } = null;

        [Required]
        [Column(TypeName = "date")]
        public DateTime CreatedAt { get; set; }

        // public Guid RoleId { get; set; }

        // public UserRole Role { get; set; } = null!;

        public List<Review> Reviews { get; set; } = new List<Review>();

        public List<Game> Games { get; set; } = new List<Game>();
    }
}