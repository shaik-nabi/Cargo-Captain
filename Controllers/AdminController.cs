using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;

namespace CargoCaptain.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeService _employeeService;

        public AdminController(ApplicationDbContext context, IEmployeeService employeeService)
        {
            _context = context;
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();

            // 1. Calculate live database statistics
            viewModel.TotalBookings = await _context.ShipmentBookings.CountAsync();
            viewModel.TotalContainers = await _context.Containers.CountAsync();

            viewModel.ActiveShipments = await _context.ShipmentBookings
                .CountAsync(sb => sb.bookingStatus != BookingStatus.COMPLETED 
                               && sb.bookingStatus != BookingStatus.CANCELLED);

            viewModel.TotalRevenue = await _context.FreightInvoices
                .Where(fi => fi.invoiceStatus == InvoiceStatus.PAID)
                .SumAsync(fi => fi.totalAmount);

            // Profit = 30% of total revenue as placeholder formula, or calculated dynamically
            viewModel.TotalProfit = viewModel.TotalRevenue * 0.30m;

            // 2. Fetch Recent listings
            viewModel.RecentBookings = await _context.ShipmentBookings
                .OrderByDescending(sb => sb.bookingId)
                .Take(5)
                .ToListAsync();

            viewModel.RecentInvoices = await _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                .OrderByDescending(fi => fi.invoiceId)
                .Take(5)
                .ToListAsync();

            // 3. Populate Chart.js dynamic data vectors
            // Retrieve data for the past 6 months from paid invoices
            var monthsList = new List<string>();
            var revenueData = new List<decimal>();
            var profitData = new List<decimal>();

            var paidInvoices = await _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                .Where(fi => fi.invoiceStatus == InvoiceStatus.PAID)
                .ToListAsync();

            var now = DateTime.UtcNow;
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var nextMonthDate = monthDate.AddMonths(1);

                var monthName = monthDate.ToString("MMM yyyy");
                monthsList.Add(monthName);

                var monthlyRev = paidInvoices
                    .Where(fi => {
                        var date = fi.paymentDate ?? fi.ShipmentBooking?.bookingDate ?? DateTime.MinValue;
                        return date >= monthDate && date < nextMonthDate;
                    })
                    .Sum(fi => fi.totalAmount);

                var monthlyProf = monthlyRev * 0.30m;

                revenueData.Add(monthlyRev);
                profitData.Add(monthlyProf);
            }

            viewModel.ChartMonths = monthsList;
            viewModel.ChartRevenueData = revenueData;
            viewModel.ChartProfitData = profitData;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Employees(
            string? search, UserRole? roleFilter, string? sortBy, bool isDescending = false, int page = 1)
        {
            const int pageSize = 5;

            var (employees, totalCount) = await _employeeService.GetEmployeesPagedAsync(
                search, roleFilter, sortBy, isDescending, page, pageSize);

            var viewModel = new EmployeeListViewModel
                {
                    Employees = employees,
                    SearchQuery = search,
                    RoleFilter = roleFilter,
                    SortBy = sortBy,
                    IsDescending = isDescending,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View(new EmployeeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Role == UserRole.Admin)
            {
                ModelState.AddModelError("Role", "Cannot create another Admin account. Only one Admin is permitted.");
                return View(model);
            }

            // Exclude Shipper & Consignee from admin employee creation if desired, but we allow operational roles
            if (model.Role == UserRole.Shipper || model.Role == UserRole.Consignee)
            {
                ModelState.AddModelError("Role", "Shipper and Consignee users are managed via guest registrations.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new employee accounts.");
                return View(model);
            }

            var employee = new Employee
            {
                firstName = model.firstName,
                lastName = model.lastName,
                email = model.email,
                phoneNumber = model.phoneNumber
            };

            try
            {
                await _employeeService.CreateEmployeeAsync(employee, model.Username, model.Password, model.Role);
                TempData["SuccessMessage"] = "Employee created successfully.";
                return RedirectToAction(nameof(Employees));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeViewModel
            {
                employeeId = employee.employeeId,
                firstName = employee.firstName,
                lastName = employee.lastName,
                email = employee.email,
                phoneNumber = employee.phoneNumber,
                Username = employee.Login?.AssociatedName ?? string.Empty,
                Role = employee.Login?.Role ?? UserRole.PortOperator
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int id, EmployeeViewModel model)
        {
            if (id != model.employeeId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingEmployee = await _employeeService.GetEmployeeByIdAsync(id);
            if (existingEmployee == null)
            {
                return NotFound();
            }

            if (model.Role == UserRole.Admin && existingEmployee.Login?.Role != UserRole.Admin)
            {
                ModelState.AddModelError("Role", "Cannot assign Admin role to another employee. There can only be one Admin.");
                return View(model);
            }

            if (existingEmployee.Login?.Role == UserRole.Admin && model.Role != UserRole.Admin)
            {
                ModelState.AddModelError("Role", "Cannot demote the Admin user. There must be exactly one Admin.");
                return View(model);
            }

            var employee = new Employee
            {
                employeeId = model.employeeId,
                firstName = model.firstName,
                lastName = model.lastName,
                email = model.email,
                phoneNumber = model.phoneNumber
            };

            try
            {
                await _employeeService.UpdateEmployeeAsync(employee, model.Username, model.Password, model.Role);
                TempData["SuccessMessage"] = "Employee updated successfully.";
                return RedirectToAction(nameof(Employees));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (employee.Login?.Role == UserRole.Admin)
            {
                TempData["ErrorMessage"] = "The system Admin account cannot be deleted. There must always be exactly one Admin.";
                return RedirectToAction(nameof(Employees));
            }

            await _employeeService.DeleteEmployeeAsync(id);
            TempData["SuccessMessage"] = "Employee deleted successfully.";
            return RedirectToAction(nameof(Employees));
        }
    }
}
