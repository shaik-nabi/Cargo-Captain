using System;
using System.Collections.Generic;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.ViewModels
{
    public class EmployeeListViewModel
    {
        public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();

        // Filters and Queries
        public string? SearchQuery { get; set; }
        public UserRole? RoleFilter { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; }

        // Pagination Properties
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
