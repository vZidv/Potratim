using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string? Nickname { get; set; }
        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string RoleColor { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? AvatarUrl { get; set; } = null;
        public DateTime RegistrationDate { get; set; }
    }
}