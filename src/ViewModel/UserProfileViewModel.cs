using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class UserProfileViewModel
    {
        public UserViewModel User { get; set; }
        // public Guid UserId { get; set; }
        // public string UserName { get; set; }
        // public string Email { get; set; }
        // public string Role { get; set; }
        // public string RoleColor { get; set; }
        // public string Status { get; set; }
        // public DateTime CreatedAt { get; set; }
        public List<GameViewModel>? GameCollection { get; set; } = new List<GameViewModel>();
    }
}