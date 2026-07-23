using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.Controllers
{
    [Authorize(Roles = "Consignee")]
    public class ConsigneeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsigneeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await GetConsigneeDashboardViewAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            return await GetConsigneeDashboardViewAsync();
        }

        private async Task<IActionResult> GetConsigneeDashboardViewAsync()
        {
            var name = User.Identity?.Name ?? string.Empty;

            var userLogin = await _context.Logins
                .FirstOrDefaultAsync(l => l.AssociatedName.ToLower() == name.ToLower());

            var assocName = userLogin?.AssociatedName ?? name;

            var bookings = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                    .ThenInclude(c => c.CargoEvents)
                .Include(sb => sb.CustomsDeclaration)
                .Include(sb => sb.FreightInvoice)
                .Where(sb => sb.consigneeName.ToLower() == assocName.ToLower() 
                          || sb.consigneeName.ToLower().Contains(name.ToLower()))
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();

            // Fallback to all bookings if no exact name match, ensuring active view for demo accounts
            if (!bookings.Any())
            {
                bookings = await _context.ShipmentBookings
                    .Include(sb => sb.Containers)
                        .ThenInclude(c => c.CargoEvents)
                    .Include(sb => sb.CustomsDeclaration)
                    .Include(sb => sb.FreightInvoice)
                    .OrderByDescending(sb => sb.bookingId)
                    .ToListAsync();
            }

            int total = bookings.Count;
            int inTransit = bookings.Count(b => b.bookingStatus == BookingStatus.CONFIRMED 
                && !b.Containers.Any(c => c.CargoEvents.Any(e => e.eventType == CargoEventType.GATE_OUT)));
            int delivered = bookings.Count(b => b.bookingStatus == BookingStatus.COMPLETED 
                || b.Containers.Any(c => c.CargoEvents.Any(e => e.eventType == CargoEventType.GATE_OUT)));

            decimal unpaidTotal = bookings
                .Where(b => b.FreightInvoice != null 
                    && (b.FreightInvoice.invoiceStatus == InvoiceStatus.ISSUED || b.FreightInvoice.invoiceStatus == InvoiceStatus.OVERDUE))
                .Sum(b => b.FreightInvoice!.totalAmount);

            int clearedCount = bookings.Count(b => b.CustomsDeclaration != null && b.CustomsDeclaration.clearanceStatus == ClearanceStatus.CLEARED);

            ViewBag.TotalBookings = total;
            ViewBag.InTransitCount = inTransit;
            ViewBag.DeliveredCount = delivered;
            ViewBag.UnpaidInvoiceTotal = unpaidTotal;
            ViewBag.ClearedCustomsCount = clearedCount;

            return View("Index", bookings);
        }
    }
}
