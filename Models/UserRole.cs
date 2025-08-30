using System.ComponentModel.DataAnnotations;

namespace Potratim.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Color { get; set; } = null!;

        public List<User> Users { get; set; } = new();
    }
}