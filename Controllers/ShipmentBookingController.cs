using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;

namespace CargoCaptain.Controllers
{
    [Authorize(Roles = "Shipper")]
    public class ShipmentBookingController : Controller
    {
        private readonly IShipmentBookingService _bookingService;

        public ShipmentBookingController(IShipmentBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("User identity claim not resolved.");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var bookings = (await _bookingService.GetShipperBookingsAsync(userId)).ToList();

            var viewModel = new ShipperDashboardViewModel
            {
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.bookingStatus == BookingStatus.PENDING),
                ConfirmedBookings = bookings.Count(b => b.bookingStatus == BookingStatus.CONFIRMED),
                CancelledBookings = bookings.Count(b => b.bookingStatus == BookingStatus.CANCELLED),
                RecentBookings = bookings.Take(5).ToList()
            };

            // Build dynamic Recent Activity Logs
            foreach (var b in bookings.Take(3))
            {
                viewModel.RecentActivities.Add($"Booking {b.bookingNumber} registered for {b.consigneeName} with status: {b.bookingStatus}.");
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BookingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            // Controller validations
            if (model.originPort?.Trim().ToLower() == model.destinationPort?.Trim().ToLower())
            {
                ModelState.AddModelError("destinationPort", "Origin Port and Destination Port cannot be the same.");
            }

            if (model.bookingDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("bookingDate", "Desired departure date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var booking = new ShipmentBooking
            {
                consigneeName = model.consigneeName,
                originPort = model.originPort!,
                destinationPort = model.destinationPort!,
                cargoWeight = model.cargoWeight,
                cargoDescription = model.cargoDescription,
                bookingDate = model.bookingDate
            };

            try
            {
                int userId = GetCurrentUserId();
                await _bookingService.CreateBookingAsync(booking, userId);
                TempData["SuccessMessage"] = "Shipment booking created successfully.";
                return RedirectToAction(nameof(Index));
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
            int userId = GetCurrentUserId();
            var booking = await _bookingService.GetBookingByIdAsync(id);
            
            // Security check: Return NotFound to prevent revealing other user's booking existence
            if (booking == null || booking.userId != userId)
            {
                return NotFound();
            }

            var viewModel = new BookingDetailsViewModel
            {
                Booking = booking,
                Containers = booking.Containers.ToList(),
                TrackingEvents = (await _bookingService.GetBookingTrackingEventsAsync(id)).ToList(),
                Invoice = booking.FreightInvoice
            };

            // Load uploaded documents list using JSON mapper file
            viewModel.UploadedDocuments = GetUploadedDocuments(id);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int userId = GetCurrentUserId();
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null || booking.userId != userId)
            {
                return NotFound();
            }

            // Edit constraint: Pending only
            if (booking.bookingStatus != BookingStatus.PENDING)
            {
                TempData["ErrorMessage"] = "Only pending bookings can be edited.";
                return RedirectToAction(nameof(Details), new { id = booking.bookingId });
            }

            var model = new BookingViewModel
            {
                bookingId = booking.bookingId,
                consigneeName = booking.consigneeName,
                originPort = booking.originPort,
                destinationPort = booking.destinationPort,
                cargoWeight = booking.cargoWeight,
                cargoDescription = booking.cargoDescription,
                bookingDate = booking.bookingDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingViewModel model)
        {
            if (id != model.bookingId)
            {
                return BadRequest();
            }

            if (model.originPort?.Trim().ToLower() == model.destinationPort?.Trim().ToLower())
            {
                ModelState.AddModelError("destinationPort", "Origin Port and Destination Port cannot be the same.");
            }

            if (model.bookingDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("bookingDate", "Desired departure date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = GetCurrentUserId();

            var booking = new ShipmentBooking
            {
                bookingId = model.bookingId,
                consigneeName = model.consigneeName,
                originPort = model.originPort!,
                destinationPort = model.destinationPort!,
                cargoWeight = model.cargoWeight,
                cargoDescription = model.cargoDescription,
                bookingDate = model.bookingDate
            };

            try
            {
                await _bookingService.UpdateBookingAsync(booking, userId);
                TempData["SuccessMessage"] = "Booking updated successfully.";
                return RedirectToAction(nameof(Details), new { id = model.bookingId });
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int userId = GetCurrentUserId();

            try
            {
                await _bookingService.CancelBookingAsync(id, userId);
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int bookingId, IFormFile file)
        {
            int userId = GetCurrentUserId();
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);

            if (booking == null || booking.userId != userId)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Details), new { id = bookingId });
            }

            // 1. File Type Validation
            var allowedExtensions = new[] { ".pdf", ".jpg", ".png", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Allowed file formats: PDF, JPG, PNG, DOCX.";
                return RedirectToAction(nameof(Details), new { id = bookingId });
            }

            // 2. File Size Validation (Max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File size cannot exceed 5MB.";
                return RedirectToAction(nameof(Details), new { id = bookingId });
            }

            // 3. Prevent Path Traversal by extracting safe filename only
            var originalFileName = Path.GetFileName(file.FileName);

            // Generate GUID Name
            var guidFileName = $"{Guid.NewGuid()}{extension}";

            // Ensure destination directory exists
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bookings", bookingId.ToString());
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var destinationPath = Path.Combine(uploadDir, guidFileName);

            // Write File to disk
            using (var stream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Record metadata mapping in JSON file
            SaveDocumentMetadata(bookingId, guidFileName, originalFileName);

            TempData["SuccessMessage"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        [HttpGet]
        public async Task<IActionResult> BillOfLading(int id)
        {
            int userId = GetCurrentUserId();
            var booking = await _bookingService.GetBookingByIdAsync(id);

            // Security guard: Return NotFound if not found or not owned by current user
            if (booking == null || booking.userId != userId)
            {
                return NotFound();
            }

            // Eligibility guard: Confirmed status check
            if (booking.bookingStatus != BookingStatus.CONFIRMED)
            {
                return BadRequest("Bill of Lading is only available for confirmed bookings.");
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            int userId = GetCurrentUserId();
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null || booking.userId != userId)
            {
                return NotFound();
            }

            var invoice = booking.FreightInvoice;
            if (invoice == null)
            {
                return BadRequest("Freight invoice has not yet been generated for this booking.");
            }

            return View(invoice);
        }

        // --- Metadata Upload Helpers ---

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

        private void SaveDocumentMetadata(int bookingId, string guidFileName, string originalFileName)
        {
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bookings", bookingId.ToString());
            var jsonPath = Path.Combine(uploadDir, "documents.json");

            var docList = GetUploadedDocuments(bookingId);
            docList.Add(new DocumentMetadata
            {
                GuidName = guidFileName,
                OriginalName = originalFileName,
                UploadTimestamp = DateTime.UtcNow
            });

            var jsonString = JsonSerializer.Serialize(docList, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(jsonPath, jsonString);
        }
    }

    public class DocumentMetadata
    {
        public string GuidName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public DateTime UploadTimestamp { get; set; }
    }
}
