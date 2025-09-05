using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Potratim.Models
{
    public class UserRole : IdentityRole<Guid>
    {
        // [Key]
        // public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Color { get; set; } = null!;

        public List<User> Users { get; set; } = new();
    }
}