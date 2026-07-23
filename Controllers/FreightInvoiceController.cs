using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;
using CargoCaptain.Data;

namespace CargoCaptain.Controllers
{
    [Authorize]
    public class FreightInvoiceController : Controller
    {
        private readonly IFreightInvoiceService _invoiceService;
        private readonly ApplicationDbContext _context;

        public FreightInvoiceController(IFreightInvoiceService invoiceService, ApplicationDbContext context)
        {
            _invoiceService = invoiceService;
            _context = context;
        }

        // --- Administrative/Forwarder Dashboard ---
        [HttpGet]
        [Authorize(Roles = "Admin,FreightForwarder")]
        public async Task<IActionResult> Index()
        {
            var invoices = (await _invoiceService.GetAllInvoicesAsync()).ToList();

            var totalRev = invoices.Where(i => i.invoiceStatus == InvoiceStatus.PAID).Sum(i => i.totalAmount);
            var outstanding = invoices.Where(i => i.invoiceStatus == InvoiceStatus.ISSUED || i.invoiceStatus == InvoiceStatus.OVERDUE).Sum(i => i.totalAmount);
            var paidInvoices = invoices.Count(i => i.invoiceStatus == InvoiceStatus.PAID);
            var unpaidInvoices = invoices.Count(i => i.invoiceStatus == InvoiceStatus.DRAFT || i.invoiceStatus == InvoiceStatus.ISSUED || i.invoiceStatus == InvoiceStatus.OVERDUE);

            var viewModel = new InvoiceDashboardViewModel
            {
                TotalRevenue = totalRev,
                OutstandingRevenue = outstanding,
                PaidRevenue = totalRev,
                PaidInvoicesCount = paidInvoices,
                UnpaidInvoicesCount = unpaidInvoices,
                RecentInvoices = invoices.Take(10).ToList()
            };

            // Gather confirmed bookings for forwarder to easily generate billing on
            ViewBag.ConfirmedBookings = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .Include(sb => sb.CustomsDeclaration)
                .Include(sb => sb.FreightInvoice)
                .Where(sb => sb.bookingStatus == BookingStatus.CONFIRMED && sb.Containers.Any())
                .ToListAsync();

            return View(viewModel);
        }

        // --- Shipper Invoices List ---
        [HttpGet]
        [Authorize(Roles = "Shipper")]
        public async Task<IActionResult> MyInvoices()
        {
            var shipperId = GetCurrentUserId();
            var invoices = await _invoiceService.GetInvoicesByShipperIdAsync(shipperId);
            return View(invoices);
        }

        // --- Generate Invoice Action ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FreightForwarder")]
        public async Task<IActionResult> Generate(int bookingId)
        {
            try
            {
                var invoice = await _invoiceService.GenerateInvoiceAsync(bookingId);
                TempData["SuccessMessage"] = $"Invoice '{invoice.invoiceNumber}' generated successfully in Draft status.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // --- Issue Invoice Action ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "FreightForwarder")]
        public async Task<IActionResult> Issue(int id)
        {
            try
            {
                await _invoiceService.IssueInvoiceAsync(id);
                TempData["SuccessMessage"] = "Invoice issued successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // --- Details Actions ---
        [HttpGet]
        [Authorize(Roles = "Admin,FreightForwarder,Shipper,Consignee")]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            // Security guard for Shippers
            if (User.IsInRole("Shipper") && invoice.ShipmentBooking?.userId != GetCurrentUserId())
            {
                return NotFound();
            }

            // Security guard for Consignees
            if (User.IsInRole("Consignee"))
            {
                var userId = GetCurrentUserId();
                var login = await _context.Logins.FindAsync(userId);
                var assocName = login?.AssociatedName ?? string.Empty;

                if (invoice.ShipmentBooking == null || 
                    (!string.Equals(invoice.ShipmentBooking.consigneeName, assocName, StringComparison.OrdinalIgnoreCase) &&
                     !invoice.ShipmentBooking.consigneeName.Contains(assocName, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }
            }

            return View(invoice);
        }

        // --- Payment Page ---
        [HttpGet]
        [Authorize(Roles = "Shipper,Consignee")]
        public async Task<IActionResult> Pay(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Shipper"))
            {
                // Ownership check
                if (invoice.ShipmentBooking?.userId != GetCurrentUserId())
                {
                    return NotFound();
                }

                if (invoice.invoiceStatus != InvoiceStatus.ISSUED && invoice.invoiceStatus != InvoiceStatus.OVERDUE)
                {
                    TempData["ErrorMessage"] = "Invoice is not in status eligible for payment.";
                    return RedirectToAction(nameof(MyInvoices));
                }

                var viewModel = new PaymentViewModel
                {
                    InvoiceId = invoice.invoiceId,
                    PaymentType = "Freight",
                    TotalAmount = invoice.freightCharges + invoice.surchargeAmount
                };

                return View(viewModel);
            }
            else // Consignee
            {
                var userId = GetCurrentUserId();
                var login = await _context.Logins.FindAsync(userId);
                var assocName = login?.AssociatedName ?? string.Empty;

                if (invoice.ShipmentBooking == null || 
                    (!string.Equals(invoice.ShipmentBooking.consigneeName, assocName, StringComparison.OrdinalIgnoreCase) &&
                     !invoice.ShipmentBooking.consigneeName.Contains(assocName, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (invoice.demurrageStatus != InvoiceStatus.ISSUED && invoice.demurrageStatus != InvoiceStatus.OVERDUE)
                {
                    TempData["ErrorMessage"] = "Demurrage charges are not in status eligible for payment.";
                    return Redirect("/Consignee/Dashboard");
                }

                var viewModel = new PaymentViewModel
                {
                    InvoiceId = invoice.invoiceId,
                    PaymentType = "Demurrage",
                    TotalAmount = invoice.demurrageAmount
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Shipper,Consignee")]
        public async Task<IActionResult> Pay(PaymentViewModel model)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(model.InvoiceId);
            if (invoice == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Shipper"))
            {
                // Ownership check
                if (invoice.ShipmentBooking?.userId != GetCurrentUserId())
                {
                    return NotFound();
                }

                model.TotalAmount = invoice.freightCharges + invoice.surchargeAmount;
                model.PaymentType = "Freight";

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    await _invoiceService.ProcessFreightPaymentAsync(model.InvoiceId, GetCurrentUserId());
                    return RedirectToAction(nameof(PaymentSuccess), new { id = model.InvoiceId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
            }
            else // Consignee
            {
                var userId = GetCurrentUserId();
                var login = await _context.Logins.FindAsync(userId);
                var assocName = login?.AssociatedName ?? string.Empty;

                if (invoice.ShipmentBooking == null || 
                    (!string.Equals(invoice.ShipmentBooking.consigneeName, assocName, StringComparison.OrdinalIgnoreCase) &&
                     !invoice.ShipmentBooking.consigneeName.Contains(assocName, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                model.TotalAmount = invoice.demurrageAmount;
                model.PaymentType = "Demurrage";

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    await _invoiceService.ProcessDemurragePaymentAsync(model.InvoiceId, GetCurrentUserId());
                    return RedirectToAction(nameof(PaymentSuccess), new { id = model.InvoiceId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
            }
        }

        // --- Payment Successful ---
        [HttpGet]
        [Authorize(Roles = "Shipper,Consignee")]
        public async Task<IActionResult> PaymentSuccess(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Shipper"))
            {
                // Ownership check
                if (invoice.ShipmentBooking?.userId != GetCurrentUserId())
                {
                    return NotFound();
                }

                if (invoice.invoiceStatus != InvoiceStatus.PAID)
                {
                    return RedirectToAction(nameof(MyInvoices));
                }
            }
            else // Consignee
            {
                var userId = GetCurrentUserId();
                var login = await _context.Logins.FindAsync(userId);
                var assocName = login?.AssociatedName ?? string.Empty;

                if (invoice.ShipmentBooking == null || 
                    (!string.Equals(invoice.ShipmentBooking.consigneeName, assocName, StringComparison.OrdinalIgnoreCase) &&
                     !invoice.ShipmentBooking.consigneeName.Contains(assocName, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                if (invoice.demurrageStatus != InvoiceStatus.PAID)
                {
                    return Redirect("/Consignee/Dashboard");
                }
            }

            return View(invoice);
        }

        // --- Helper Loader ---

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("User identity claim not resolved.");
        }
    }
}
