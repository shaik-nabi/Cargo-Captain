using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.Enums;

namespace CargoCaptain.Services
{
    public class ShipmentBookingService : IShipmentBookingService
    {
        private readonly ApplicationDbContext _context;

        public ShipmentBookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShipmentBooking>> GetShipperBookingsAsync(int userId)
        {
            return await _context.ShipmentBookings
                .Where(sb => sb.userId == userId)
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();
        }

        public async Task<ShipmentBooking?> GetBookingByIdAsync(int id)
        {
            return await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .Include(sb => sb.CustomsDeclaration)
                .Include(sb => sb.FreightInvoice)
                .FirstOrDefaultAsync(sb => sb.bookingId == id);
        }

        public async Task CreateBookingAsync(ShipmentBooking booking, int userId)
        {
            var login = await _context.Logins.FindAsync(userId);
            if (login == null)
            {
                throw new InvalidOperationException("User account not found.");
            }

            // Enforce server-side validations
            ValidateBooking(booking);

            // Generate unique booking number
            string bookingNum;
            do
            {
                bookingNum = GenerateBookingNumber();
            } while (await BookingNumberExistsAsync(bookingNum));

            booking.bookingNumber = bookingNum;
            booking.userId = userId;
            booking.shipperName = login.AssociatedName; // Bind to logged-in user profile
            booking.bookingStatus = BookingStatus.PENDING;

            _context.ShipmentBookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookingAsync(ShipmentBooking booking, int userId)
        {
            var dbBooking = await _context.ShipmentBookings.FindAsync(booking.bookingId);
            if (dbBooking == null)
            {
                throw new KeyNotFoundException("Booking record not found.");
            }

            // Security guard: ownership check
            if (dbBooking.userId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this booking.");
            }

            // Status boundary guard
            if (dbBooking.bookingStatus != BookingStatus.PENDING)
            {
                throw new InvalidOperationException("Modification is only allowed for bookings in Pending status.");
            }

            // Validate new data parameters
            ValidateBooking(booking);

            // Update allowed fields
            dbBooking.consigneeName = booking.consigneeName;
            dbBooking.originPort = booking.originPort;
            dbBooking.destinationPort = booking.destinationPort;
            dbBooking.cargoWeight = booking.cargoWeight;
            dbBooking.cargoDescription = booking.cargoDescription;
            dbBooking.bookingDate = booking.bookingDate;

            _context.Entry(dbBooking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task CancelBookingAsync(int id, int userId)
        {
            var dbBooking = await _context.ShipmentBookings.FindAsync(id);
            if (dbBooking == null)
            {
                throw new KeyNotFoundException("Booking record not found.");
            }

            // Security guard: ownership check
            if (dbBooking.userId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to cancel this booking.");
            }

            // Status boundary guard
            if (dbBooking.bookingStatus != BookingStatus.PENDING)
            {
                throw new InvalidOperationException("Cancellation is only allowed for bookings in Pending status.");
            }

            dbBooking.bookingStatus = BookingStatus.CANCELLED;
            _context.Entry(dbBooking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> BookingNumberExistsAsync(string bookingNumber)
        {
            return await _context.ShipmentBookings.AnyAsync(sb => sb.bookingNumber.ToLower() == bookingNumber.ToLower());
        }

        public async Task<IEnumerable<CargoEvent>> GetBookingTrackingEventsAsync(int bookingId)
        {
            return await _context.CargoEvents
                .Include(ce => ce.Container)
                .Where(ce => ce.Container != null && ce.Container.bookingId == bookingId)
                .OrderBy(ce => ce.eventTimestamp)
                .ToListAsync();
        }

        public async Task<FreightInvoice?> GetBookingInvoiceAsync(int bookingId)
        {
            return await _context.FreightInvoices
                .FirstOrDefaultAsync(fi => fi.bookingId == bookingId);
        }

        // --- Helper Methods ---

        private void ValidateBooking(ShipmentBooking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.consigneeName))
            {
                throw new ArgumentException("Consignee Name is required.");
            }
            if (string.IsNullOrWhiteSpace(booking.originPort) || string.IsNullOrWhiteSpace(booking.destinationPort))
            {
                throw new ArgumentException("Origin and Destination Ports are required.");
            }
            if (booking.originPort.Trim().ToLower() == booking.destinationPort.Trim().ToLower())
            {
                throw new ArgumentException("Origin Port and Destination Port cannot be the same.");
            }
            if (booking.cargoWeight <= 0)
            {
                throw new ArgumentException("Cargo weight must be greater than zero.");
            }
            if (string.IsNullOrWhiteSpace(booking.cargoDescription))
            {
                throw new ArgumentException("Cargo description is required.");
            }
            
        }

        private string GenerateBookingNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = GetRandomAlphanumeric(4);
            return $"BKG-{datePart}-{randomPart}";
        }

        private string GetRandomAlphanumeric(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
