using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.Interfaces
{
    public interface IEmployeeService
    {
        Task<(IEnumerable<Employee> Employees, int TotalCount)> GetEmployeesPagedAsync(
            string? search, UserRole? roleFilter, string? sortBy, bool isDescending, int page, int pageSize);

        Task<Employee?> GetEmployeeByIdAsync(int id);
        
        Task CreateEmployeeAsync(Employee employee, string username, string password, UserRole role);
        
        Task UpdateEmployeeAsync(Employee employee, string username, string? password, UserRole role);
        
        Task DeleteEmployeeAsync(int id);

        Task<bool> EmployeeExistsAsync(int id);
        
        Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null);
        
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
    }
}
