using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.Enums;

namespace CargoCaptain.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Employee> Employees, int TotalCount)> GetEmployeesPagedAsync(
            string? search, UserRole? roleFilter, string? sortBy, bool isDescending, int page, int pageSize)
        {
            var query = _context.Employees
                .Include(e => e.Login)
                .AsQueryable();

            // 1. Search filter (Name or Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(e => e.firstName.ToLower().Contains(search) 
                                      || e.lastName.ToLower().Contains(search) 
                                      || e.email.ToLower().Contains(search));
            }

            // 2. Role filter
            if (roleFilter.HasValue)
            {
                query = query.Where(e => e.Login != null && e.Login.Role == roleFilter.Value);
            }

            // 3. Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "firstname":
                        query = isDescending ? query.OrderByDescending(e => e.firstName) : query.OrderBy(e => e.firstName);
                        break;
                    case "lastname":
                        query = isDescending ? query.OrderByDescending(e => e.lastName) : query.OrderBy(e => e.lastName);
                        break;
                    case "email":
                        query = isDescending ? query.OrderByDescending(e => e.email) : query.OrderBy(e => e.email);
                        break;
                    case "role":
                        query = isDescending ? query.OrderByDescending(e => e.Login!.Role) : query.OrderBy(e => e.Login!.Role);
                        break;
                    default:
                        query = query.OrderBy(e => e.employeeId);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(e => e.employeeId);
            }

            // 4. Pagination counts
            int totalCount = await query.CountAsync();
            var pagedEmployees = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (pagedEmployees, totalCount);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Login)
                .FirstOrDefaultAsync(e => e.employeeId == id);
        }

        public async Task CreateEmployeeAsync(Employee employee, string username, string password, UserRole role)
        {
            // Validations inside the service
            if (await EmailExistsAsync(employee.email))
            {
                throw new InvalidOperationException("Email address is already in use by another employee.");
            }

            if (await UsernameExistsAsync(username))
            {
                throw new InvalidOperationException("Username is already in use by another user account.");
            }

            // Create login entity first
            var newLogin = new Login
            {
                AssociatedName = username,
                Role = role
            };

            var hasher = new PasswordHasher<Login>();
            newLogin.Password = hasher.HashPassword(newLogin, password);

            _context.Logins.Add(newLogin);
            await _context.SaveChangesAsync();

            // Assign FK
            employee.userId = newLogin.UserId;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(Employee employee, string username, string? password, UserRole role)
        {
            // Find existing entities
            var dbEmployee = await _context.Employees.FindAsync(employee.employeeId);
            if (dbEmployee == null)
            {
                throw new KeyNotFoundException("Employee record not found.");
            }

            var dbLogin = await _context.Logins.FindAsync(dbEmployee.userId);
            if (dbLogin == null)
            {
                throw new KeyNotFoundException("Associated login account not found.");
            }

            // Validations inside the service
            if (await EmailExistsAsync(employee.email, employee.employeeId))
            {
                throw new InvalidOperationException("Email address is already in use by another employee.");
            }

            if (await UsernameExistsAsync(username, dbEmployee.userId))
            {
                throw new InvalidOperationException("Username is already in use by another user account.");
            }

            // Update Employee properties
            dbEmployee.firstName = employee.firstName;
            dbEmployee.lastName = employee.lastName;
            dbEmployee.email = employee.email;
            dbEmployee.phoneNumber = employee.phoneNumber;

            // Update Login properties
            dbLogin.AssociatedName = username;
            dbLogin.Role = role;

            if (!string.IsNullOrWhiteSpace(password))
            {
                var hasher = new PasswordHasher<Login>();
                dbLogin.Password = hasher.HashPassword(dbLogin, password);
            }

            _context.Entry(dbEmployee).State = EntityState.Modified;
            _context.Entry(dbLogin).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return;

            var login = await _context.Logins.FindAsync(employee.userId);
            
            // Removing Employee (Cascades or drops login as well, we remove both for consistency)
            _context.Employees.Remove(employee);
            if (login != null)
            {
                _context.Logins.Remove(login);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmployeeExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.employeeId == id);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null)
        {
            if (excludeEmployeeId.HasValue)
            {
                return await _context.Employees.AnyAsync(e => e.email.ToLower() == email.ToLower() && e.employeeId != excludeEmployeeId.Value);
            }
            return await _context.Employees.AnyAsync(e => e.email.ToLower() == email.ToLower());
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            if (excludeUserId.HasValue)
            {
                return await _context.Logins.AnyAsync(l => l.AssociatedName.ToLower() == username.ToLower() && l.UserId != excludeUserId.Value);
            }
            return await _context.Logins.AnyAsync(l => l.AssociatedName.ToLower() == username.ToLower());
        }
    }
}
