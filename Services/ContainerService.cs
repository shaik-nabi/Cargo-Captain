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
    public class ContainerService : IContainerService
    {
        private readonly ApplicationDbContext _context;

        public ContainerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShipmentBooking>> GetAllBookingsAsync()
        {
            return await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShipmentBooking>> GetPendingAllocationsAsync()
        {
            // Pending Allocation = Shipment bookings that have zero containers allocated
            return await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .Where(sb => !sb.Containers.Any())
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShipmentBooking>> GetCompletedAllocationsAsync()
        {
            // Completed Allocation = Shipment bookings that have at least one container allocated
            return await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .Where(sb => sb.Containers.Any())
                .OrderByDescending(sb => sb.bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Container>> GetContainersByBookingIdAsync(int bookingId)
        {
            return await _context.Containers
                .Where(c => c.bookingId == bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Container>> GetRecentAllocationsAsync(int count)
        {
            return await _context.Containers
                .Include(c => c.ShipmentBooking)
                .OrderByDescending(c => c.containerId)
                .Take(count)
                .ToListAsync();
        }

        public async Task AllocateContainerAsync(Container container)
        {
            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .FirstOrDefaultAsync(sb => sb.bookingId == container.bookingId);

            if (booking == null)
            {
                throw new KeyNotFoundException("Booking record not found.");
            }

            // Status checks
            if (booking.bookingStatus == BookingStatus.CANCELLED)
            {
                throw new InvalidOperationException("Cannot allocate containers to a cancelled booking.");
            }
            if (booking.bookingStatus == BookingStatus.COMPLETED)
            {
                throw new InvalidOperationException("Cannot allocate containers to a completed booking.");
            }

            // Validate unique container number
            if (await ContainerNumberExistsAsync(container.containerNumber))
            {
                throw new InvalidOperationException($"Container number '{container.containerNumber}' is already registered in the system database.");
            }

            _context.Containers.Add(container);

            if (booking.bookingStatus == BookingStatus.PENDING)
            {
                booking.bookingStatus = BookingStatus.CONFIRMED;
                _context.Entry(booking).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveContainerAsync(int containerId)
        {
            var container = await _context.Containers.FindAsync(containerId);
            if (container == null) return;

            var booking = await _context.ShipmentBookings.FindAsync(container.bookingId);
            if (booking == null)
            {
                throw new KeyNotFoundException("Associated booking record not found.");
            }

            // Removal check constraint: PENDING only
            if (booking.bookingStatus != BookingStatus.PENDING)
            {
                throw new InvalidOperationException("Container removal is only allowed while the booking status is Pending.");
            }

            _context.Containers.Remove(container);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ContainerNumberExistsAsync(string containerNumber, int? excludeContainerId = null)
        {
            if (excludeContainerId.HasValue)
            {
                return await _context.Containers.AnyAsync(c => c.containerNumber.ToLower() == containerNumber.ToLower() && c.containerId != excludeContainerId.Value);
            }
            return await _context.Containers.AnyAsync(c => c.containerNumber.ToLower() == containerNumber.ToLower());
        }

        public async Task<Container?> GetContainerByIdAsync(int containerId)
        {
            return await _context.Containers
                .Include(c => c.ShipmentBooking)
                .FirstOrDefaultAsync(c => c.containerId == containerId);
        }

        public async Task<ShipmentBooking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                .FirstOrDefaultAsync(sb => sb.bookingId == bookingId);
        }
    }
}
