using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Potratim.ViewModel
{
    public class UserSelfEditViewModel
    {
        public Guid Id { get; set; }
        [Required]
        public string? Nickname { get; set; }
        [Required]
        public string Email { get; set; } = null!;

        public string? AvatarUrl { get; set; } = null;
    }
}