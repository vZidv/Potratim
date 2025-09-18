using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class UserEditViewModel
    {
        public UserViewModel User { get; set; }
        public List<string> Roles { get; set; }
    }
}