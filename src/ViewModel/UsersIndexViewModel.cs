using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Potratim.Models;
using X.PagedList;

namespace Potratim.ViewModel
{
    public class UsersIndexViewModel
    {
        public IPagedList<UserViewModel> Users { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();

        //Filters
        public string SearchTerm { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "Id";
        public string? StatusFilter { get; set; } = string.Empty;
        public string? RoleFilter { get; set; } = string.Empty;
        public DateTime? RegistrationDateFrom { get; set; }
        public DateTime? RegistrationDateTo { get; set; }

        //Support properties
        public int AllUsersCount { get; set; } = 0;
        public int ClientsCount { get; set; } = 0;
        public int ModeratorsCount { get; set; } = 0;
        public int AdminsCount { get; set; } = 0;
    }
}