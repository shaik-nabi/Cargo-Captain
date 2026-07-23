using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    [Authorize(Roles = "CustomsBroker")]
    public class CustomsController : Controller
    {
        private readonly ICustomsService _customsService;
        private readonly ApplicationDbContext _context;

        public CustomsController(ICustomsService customsService, ApplicationDbContext context)
        {
            _customsService = customsService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var entries = await GetAllCustomsEntriesAsync();

            var viewModel = new CustomsDashboardViewModel
            {
                TotalSubmissions = entries.Count,
                PendingActionCount = entries.Count(e => !e.IsFiledDeclaration || e.ClearanceStatus == ClearanceStatus.FILED || e.ClearanceStatus == ClearanceStatus.UNDER_REVIEW),
                ApprovedCount = entries.Count(e => e.ClearanceStatus == ClearanceStatus.CLEARED),
                HeldCount = entries.Count(e => e.ClearanceStatus == ClearanceStatus.HELD || e.ClearanceStatus == ClearanceStatus.REJECTED),
                RecentEntries = entries.Take(5).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Declarations(string? filter = "all")
        {
            ViewBag.ActiveFilter = filter;

            var entries = await GetAllCustomsEntriesAsync();

            IEnumerable<CustomsEntryViewModel> filteredEntries = filter?.ToLower() switch
            {
                "pending" => entries.Where(e => !e.IsFiledDeclaration || e.ClearanceStatus == ClearanceStatus.FILED || e.ClearanceStatus == ClearanceStatus.UNDER_REVIEW),
                "completed" => entries.Where(e => e.ClearanceStatus == ClearanceStatus.CLEARED || e.ClearanceStatus == ClearanceStatus.HELD || e.ClearanceStatus == ClearanceStatus.REJECTED),
                _ => entries
            };

            return View(filteredEntries);
        }

        private async Task<List<CustomsEntryViewModel>> GetAllCustomsEntriesAsync()
        {
            var declarations = (await _customsService.GetAllDeclarationsAsync()).ToList();

            var activeBookings = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .Include(sb => sb.CustomsDeclaration)
                .Where(sb => sb.bookingStatus != BookingStatus.CANCELLED)
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();

            var entries = new List<CustomsEntryViewModel>();

            // Add filed declarations first
            foreach (var cd in declarations)
            {
                entries.Add(new CustomsEntryViewModel
                {
                    IsFiledDeclaration = true,
                    DeclarationId = cd.declarationId,
                    BookingId = cd.bookingId,
                    BookingNumber = cd.ShipmentBooking?.bookingNumber ?? $"BKG-{cd.bookingId}",
                    ShipperName = cd.ShipmentBooking?.shipperName ?? "Shipper",
                    ConsigneeName = cd.ShipmentBooking?.consigneeName ?? "Consignee",
                    OriginPort = cd.ShipmentBooking?.originPort ?? "",
                    DestinationPort = cd.ShipmentBooking?.destinationPort ?? "",
                    ContainerCount = cd.ShipmentBooking?.Containers.Count ?? 0,
                    Documents = GetUploadedDocuments(cd.bookingId),
                    DeclarationType = cd.declarationType,
                    HsCode = cd.hsCode,
                    DeclaredValue = cd.declaredValue,
                    CalculatedDuty = cd.calculatedDuty,
                    ClearanceStatus = cd.clearanceStatus,
                    BookingStatus = cd.ShipmentBooking?.bookingStatus ?? BookingStatus.CONFIRMED,
                    CreatedDate = cd.ShipmentBooking?.bookingDate ?? DateTime.UtcNow
                });
            }

            // Add active bookings without filed declarations
            foreach (var b in activeBookings.Where(b => b.CustomsDeclaration == null))
            {
                var docs = GetUploadedDocuments(b.bookingId);
                entries.Add(new CustomsEntryViewModel
                {
                    IsFiledDeclaration = false,
                    BookingId = b.bookingId,
                    BookingNumber = b.bookingNumber,
                    ShipperName = b.shipperName,
                    ConsigneeName = b.consigneeName,
                    OriginPort = b.originPort,
                    DestinationPort = b.destinationPort,
                    ContainerCount = b.Containers.Count,
                    Documents = docs,
                    BookingStatus = b.bookingStatus,
                    CreatedDate = b.bookingDate
                });
            }

            return entries;
        }

        [HttpGet]
        public async Task<IActionResult> File(int bookingId)
        {
            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .FirstOrDefaultAsync(sb => sb.bookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Prerequisite checks before rendering form
            if (booking.bookingStatus == BookingStatus.CANCELLED || booking.bookingStatus == BookingStatus.COMPLETED)
            {
                TempData["ErrorMessage"] = "Filing is only allowed for active bookings.";
                return Redirect("/Customs/Bookings");
            }
            if (!await _customsService.VerifyBookingDocumentsExistAsync(bookingId))
            {
                TempData["ErrorMessage"] = "Filing is blocked: required shipping/customs documentation is missing.";
                return Redirect("/Customs/Bookings");
            }

            ViewBag.UploadedDocuments = GetUploadedDocuments(booking.bookingId);

            var viewModel = new FileDeclarationViewModel
            {
                bookingId = booking.bookingId,
                bookingNumber = booking.bookingNumber
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> File(FileDeclarationViewModel model)
        {
            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .FirstOrDefaultAsync(sb => sb.bookingId == model.bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            model.bookingNumber = booking.bookingNumber;
            ViewBag.UploadedDocuments = GetUploadedDocuments(booking.bookingId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prerequisite checks
            if (booking.bookingStatus == BookingStatus.CANCELLED || booking.bookingStatus == BookingStatus.COMPLETED || !await _customsService.VerifyBookingDocumentsExistAsync(model.bookingId))
            {
                ModelState.AddModelError(string.Empty, "Filing prerequisites are not satisfied.");
                return View(model);
            }

            var declaration = new CustomsDeclaration
            {
                bookingId = model.bookingId,
                hsCode = model.hsCode.Trim(),
                declaredValue = model.declaredValue,
                declarationType = model.declarationType
            };

            try
            {
                await _customsService.FileDeclarationAsync(declaration);
                TempData["SuccessMessage"] = "Customs declaration filed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var declaration = await _customsService.GetDeclarationByIdAsync(id);
            if (declaration == null)
            {
                return NotFound();
            }

            // Load original files uploaded by shipper using JSON metadata loader helper
            ViewBag.UploadedDocuments = GetUploadedDocuments(declaration.bookingId);

            return View(declaration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ClearanceStatus newStatus)
        {
            try
            {
                await _customsService.UpdateDeclarationStatusAsync(id, newStatus);
                TempData["SuccessMessage"] = "Declaration clearance status updated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                    .ThenInclude(c => c.CargoEvents)
                .Include(sb => sb.CustomsDeclaration)
                .Where(sb => sb.bookingStatus != BookingStatus.CANCELLED)
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();

            var docsMap = new Dictionary<int, List<DocumentMetadata>>();
            foreach (var b in bookings)
            {
                docsMap[b.bookingId] = GetUploadedDocuments(b.bookingId);
            }
            ViewBag.DocumentsMap = docsMap;

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransitionToImport(int bookingId)
        {
            try
            {
                await _customsService.TransitionToImportCustomsAsync(bookingId);
                TempData["SuccessMessage"] = "Shipment transitioned to Import Customs Clearance phase successfully. Please review and clear the import filing.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Bookings));
        }

        // --- Helper Loader ---

        private List<DocumentMetadata> GetUploadedDocuments(int bookingId)
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bookings", bookingId.ToString(), "documents.json");
            if (!System.IO.File.Exists(jsonPath))
            {
                return new List<DocumentMetadata>();
            }

            try
            {
                var jsonContent = System.IO.File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<List<DocumentMetadata>>(jsonContent) ?? new List<DocumentMetadata>();
            }
            catch
            {
                return new List<DocumentMetadata>();
            }
        }
    }
}
