using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.Enums;
using CargoCaptain.Controllers;

namespace CargoCaptain.Services
{
    public class CustomsService : ICustomsService
    {
        private readonly ApplicationDbContext _context;

        public CustomsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomsDeclaration>> GetAllDeclarationsAsync()
        {
            return await _context.CustomsDeclarations
                .Include(cd => cd.ShipmentBooking)
                .OrderByDescending(cd => cd.declarationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomsDeclaration>> GetPendingDeclarationsAsync()
        {
            // Pending/Review state: status is FILED or UNDER_REVIEW
            return await _context.CustomsDeclarations
                .Include(cd => cd.ShipmentBooking)
                .Where(cd => cd.clearanceStatus == ClearanceStatus.FILED || cd.clearanceStatus == ClearanceStatus.UNDER_REVIEW)
                .OrderByDescending(cd => cd.declarationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomsDeclaration>> GetCompletedDeclarationsAsync()
        {
            // Completed state: CLEARED, HELD, or REJECTED
            return await _context.CustomsDeclarations
                .Include(cd => cd.ShipmentBooking)
                .Where(cd => cd.clearanceStatus == ClearanceStatus.CLEARED 
                          || cd.clearanceStatus == ClearanceStatus.HELD 
                          || cd.clearanceStatus == ClearanceStatus.REJECTED)
                .OrderByDescending(cd => cd.declarationId)
                .ToListAsync();
        }

        public async Task<CustomsDeclaration?> GetDeclarationByIdAsync(int id)
        {
            return await _context.CustomsDeclarations
                .Include(cd => cd.ShipmentBooking)
                .FirstOrDefaultAsync(cd => cd.declarationId == id);
        }

        public async Task<CustomsDeclaration?> GetDeclarationByBookingIdAsync(int bookingId)
        {
            return await _context.CustomsDeclarations
                .Include(cd => cd.ShipmentBooking)
                .FirstOrDefaultAsync(cd => cd.bookingId == bookingId);
        }

        public async Task FileDeclarationAsync(CustomsDeclaration declaration)
        {
            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .FirstOrDefaultAsync(sb => sb.bookingId == declaration.bookingId);

            if (booking == null)
            {
                throw new KeyNotFoundException("Booking record not found.");
            }

            // 1. Customs Eligibility Prerequisites
            if (booking.bookingStatus == BookingStatus.CANCELLED || booking.bookingStatus == BookingStatus.COMPLETED)
            {
                throw new InvalidOperationException("Customs declaration filing is not permitted for cancelled or completed bookings.");
            }

            // 2. Document Verification Check
            if (!await VerifyBookingDocumentsExistAsync(declaration.bookingId))
            {
                throw new InvalidOperationException("Required shipping documentation (such as packing lists, B/Ls) is missing. Declaration cannot be submitted.");
            }

            if (booking.bookingStatus == BookingStatus.PENDING)
            {
                booking.bookingStatus = BookingStatus.CONFIRMED;
                _context.Entry(booking).State = EntityState.Modified;
            }

            // 3. One Declaration of this type per booking check
            bool exists = await _context.CustomsDeclarations
                .AnyAsync(cd => cd.bookingId == declaration.bookingId && cd.declarationType == declaration.declarationType);
            if (exists)
            {
                throw new InvalidOperationException($"A customs declaration of type '{declaration.declarationType}' has already been filed for this booking.");
            }

            // 4. Validate HS Code format
            if (!await ValidateHSCodeAsync(declaration.hsCode))
            {
                throw new ArgumentException("HS Code must be a numeric string between 6 and 10 digits.");
            }

            // 5. Duty calculation in service
            declaration.calculatedDuty = await CalculateDutyAsync(declaration.hsCode, declaration.declaredValue);
            declaration.clearanceStatus = ClearanceStatus.FILED; // Starts in filed/pending state

            _context.CustomsDeclarations.Add(declaration);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeclarationStatusAsync(int id, ClearanceStatus newStatus)
        {
            var declaration = await _context.CustomsDeclarations.FindAsync(id);
            if (declaration == null)
            {
                throw new KeyNotFoundException("Customs declaration record not found.");
            }

            var currentStatus = declaration.clearanceStatus;

            // Strict workflow state transition validations
            if (currentStatus == ClearanceStatus.FILED)
            {
                if (newStatus != ClearanceStatus.UNDER_REVIEW)
                {
                    throw new InvalidOperationException("Filed declarations must be set to Under Review status before final clearance or rejection.");
                }
            }
            else if (currentStatus == ClearanceStatus.UNDER_REVIEW)
            {
                var allowed = new[] { ClearanceStatus.CLEARED, ClearanceStatus.REJECTED, ClearanceStatus.HELD };
                if (!allowed.Contains(newStatus))
                {
                    throw new InvalidOperationException("Under Review declarations can only transition to Cleared (Approved), Rejected, or Held status.");
                }
            }
            else if (currentStatus == ClearanceStatus.CLEARED || currentStatus == ClearanceStatus.REJECTED)
            {
                if (newStatus != currentStatus)
                {
                    throw new InvalidOperationException("Approved or Rejected declarations are finalized and cannot be modified.");
                }
            }
            else if (currentStatus == ClearanceStatus.HELD)
            {
                var allowed = new[] { ClearanceStatus.UNDER_REVIEW, ClearanceStatus.CLEARED, ClearanceStatus.REJECTED };
                if (!allowed.Contains(newStatus))
                {
                    throw new InvalidOperationException("Held declarations can transition back to Under Review, Cleared (Approved), or Rejected status.");
                }
            }

            declaration.clearanceStatus = newStatus;
            _context.Entry(declaration).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public Task<decimal> CalculateDutyAsync(string hsCode, decimal declaredValue)
        {
            if (string.IsNullOrWhiteSpace(hsCode) || hsCode.Length < 2)
            {
                return Task.FromResult(declaredValue * 0.06m);
            }

            var prefix = hsCode.Substring(0, 2);
            decimal rate = 0.06m; // Default tariff rate: 6%

            if (prefix == "84" || prefix == "85")
            {
                rate = 0.05m; // Electronics: 5%
            }
            else if (int.TryParse(prefix, out int pInt) && pInt >= 50 && pInt <= 63)
            {
                rate = 0.10m; // Textiles: 10%
            }
            else if (pInt >= 28 && pInt <= 38)
            {
                rate = 0.08m; // Chemicals: 8%
            }
            else if (prefix == "87")
            {
                rate = 0.12m; // Automotive: 12%
            }

            return Task.FromResult(declaredValue * rate);
        }

        public Task<bool> ValidateHSCodeAsync(string hsCode)
        {
            if (string.IsNullOrWhiteSpace(hsCode)) return Task.FromResult(false);
            
            // Check numeric format and length parameters
            bool isNumeric = hsCode.All(char.IsDigit);
            bool isCorrectLength = hsCode.Length >= 6 && hsCode.Length <= 10;
            
            return Task.FromResult(isNumeric && isCorrectLength);
        }

        public Task<bool> VerifyBookingDocumentsExistAsync(int bookingId)
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bookings", bookingId.ToString(), "documents.json");
            if (!File.Exists(jsonPath))
            {
                return Task.FromResult(false);
            }

            try
            {
                var jsonContent = File.ReadAllText(jsonPath);
                var docList = JsonSerializer.Deserialize<List<DocumentMetadata>>(jsonContent);
                return Task.FromResult(docList != null && docList.Any());
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task TransitionToImportCustomsAsync(int bookingId)
        {
            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                    .ThenInclude(c => c!.CargoEvents)
                .Include(sb => sb.CustomsDeclaration)
                .FirstOrDefaultAsync(sb => sb.bookingId == bookingId);

            if (booking == null)
            {
                throw new KeyNotFoundException("Booking record not found.");
            }

            var customs = booking.CustomsDeclaration;
            if (customs == null || customs.declarationType != DeclarationType.EXPORT || customs.clearanceStatus != ClearanceStatus.CLEARED)
            {
                throw new InvalidOperationException("Import clearance cannot be filed: Export clearance must be completed and approved first.");
            }

            // Check if discharged port milestone has been recorded on any container
            bool discharged = booking.Containers.Any(c => c.CargoEvents != null && c.CargoEvents.Any(e => e.eventType == CargoEventType.DISCHARGED));
            if (!discharged)
            {
                throw new InvalidOperationException("Import clearance is blocked: shipment cargo has not reached the 'Discharged' destination milestone.");
            }

            // Transition type to IMPORT and status to FILED
            customs.declarationType = DeclarationType.IMPORT;
            customs.clearanceStatus = ClearanceStatus.FILED;

            _context.Entry(customs).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
