using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;

namespace Potratim.ViewModel
{
    public class UsersIndexViewModel
    {
        public List<User> Users { get; set; } = new();

        //Filters
        public string SearchTerm { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "Id";
        public string RoleFilter { get; set; } = string.Empty;

        //Support properties
        public int AllUsersCount { get; set; } = 0;
        public int ClientsCount { get; set; } = 0;
        public int ModeratorsCount { get; set; } = 0;
        public int AdminsCount { get; set; } = 0;
    }
}