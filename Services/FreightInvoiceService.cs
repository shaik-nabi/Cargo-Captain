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
    public class FreightInvoiceService : IFreightInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public FreightInvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FreightInvoice>> GetAllInvoicesAsync()
        {
            return await _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                    .ThenInclude(sb => sb.Containers)
                .OrderByDescending(fi => fi.invoiceId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FreightInvoice>> GetInvoicesByShipperIdAsync(int shipperUserId)
        {
            return await _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                    .ThenInclude(sb => sb.Containers)
                .Where(fi => fi.ShipmentBooking != null && fi.ShipmentBooking.userId == shipperUserId)
                .OrderByDescending(fi => fi.invoiceId)
                .ToListAsync();
        }

        public async Task<FreightInvoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                    .ThenInclude(sb => sb.Containers)
                .FirstOrDefaultAsync(fi => fi.invoiceId == id);
        }

        public async Task<FreightInvoice?> GetInvoiceByBookingIdAsync(int bookingId)
        {
            return await _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                    .ThenInclude(sb => sb.Containers)
                .FirstOrDefaultAsync(fi => fi.bookingId == bookingId);
        }

        public async Task<FreightInvoice> GenerateInvoiceAsync(int bookingId)
        {
            var booking = await _context.ShipmentBookings
                .Include(sb => sb.Containers)
                    .ThenInclude(c => c.CargoEvents)
                .Include(sb => sb.CustomsDeclaration)
                .FirstOrDefaultAsync(sb => sb.bookingId == bookingId);

            if (booking == null)
            {
                throw new KeyNotFoundException("Booking record not found.");
            }

            // 1. Uniqueness check
            bool exists = await _context.FreightInvoices.AnyAsync(fi => fi.bookingId == bookingId);
            if (exists)
            {
                throw new InvalidOperationException("An invoice has already been generated for this booking.");
            }

            // 2. Booking confirmed check
            if (booking.bookingStatus != BookingStatus.CONFIRMED)
            {
                throw new InvalidOperationException("Invoice generation is blocked: shipment booking must be Confirmed.");
            }

            // 3. Container allocation check
            if (booking.Containers == null || !booking.Containers.Any())
            {
                throw new InvalidOperationException("Invoice generation is blocked: container allocations are not complete.");
            }

            // 4. Customs cleared check
            var customs = booking.CustomsDeclaration;
            if (customs == null || customs.clearanceStatus != ClearanceStatus.CLEARED)
            {
                throw new InvalidOperationException("Invoice generation is blocked: Customs clearance is not Approved.");
            }

            // 5. Milestone checkpoint verification
            var decType = customs.declarationType;
            bool milestoneReached = false;

            if (decType == DeclarationType.EXPORT)
            {
                // Must have SAILED event across containers
                milestoneReached = booking.Containers.Any(c => c.CargoEvents != null && c.CargoEvents.Any(e => e.eventType == CargoEventType.SAILED));
                if (!milestoneReached)
                {
                    throw new InvalidOperationException("Invoice generation is blocked: shipment must reach the 'Sailed' port milestone before billing.");
                }
            }
            else // IMPORT
            {
                // Must have GATE_OUT event across containers
                milestoneReached = booking.Containers.Any(c => c.CargoEvents != null && c.CargoEvents.Any(e => e.eventType == CargoEventType.GATE_OUT));
                if (!milestoneReached)
                {
                    throw new InvalidOperationException("Invoice generation is blocked: shipment must reach the 'Gate Out' port milestone before billing.");
                }
            }

            // 6. Demurrage Calculation
            decimal totalDemurrage = 0m;
            foreach (var c in booking.Containers)
            {
                if (c.CargoEvents == null) continue;

                double days = 0;
                if (decType == DeclarationType.EXPORT)
                {
                    var gateInEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.GATE_IN);
                    var sailedEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.SAILED);
                    if (gateInEvent != null && sailedEvent != null)
                    {
                        days = (sailedEvent.eventTimestamp - gateInEvent.eventTimestamp).TotalDays;
                    }
                }
                else // IMPORT
                {
                    var dischargedEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.DISCHARGED);
                    var gateOutEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.GATE_OUT);
                    if (dischargedEvent != null && gateOutEvent != null)
                    {
                        days = (gateOutEvent.eventTimestamp - dischargedEvent.eventTimestamp).TotalDays;
                    }
                }

                int stagingDays = Math.Max(0, (int)Math.Ceiling(days));
                int chargeableDays = Math.Max(0, stagingDays - 3); // First 3 days free
                totalDemurrage += chargeableDays * 50m;
            }

            // 7. Charges Calculations
            decimal oceanFreight = booking.cargoWeight * 300m;
            decimal surcharges = booking.Containers.Count * 150m;

            var invoice = new FreightInvoice
            {
                bookingId = bookingId,
                freightCharges = oceanFreight,
                surchargeAmount = surcharges,
                demurrageAmount = totalDemurrage,
                totalAmount = oceanFreight + surcharges + totalDemurrage,
                currency = "USD",
                invoiceStatus = InvoiceStatus.DRAFT,
                demurrageStatus = InvoiceStatus.DRAFT,
                invoiceNumber = await GenerateUniqueInvoiceNumberAsync()
            };

            _context.FreightInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task IssueInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.FreightInvoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                throw new KeyNotFoundException("Invoice record not found.");
            }

            // Status sequence check
            if (invoice.invoiceStatus != InvoiceStatus.DRAFT)
            {
                throw new InvalidOperationException("Only Draft invoices can be Issued.");
            }

            invoice.invoiceStatus = InvoiceStatus.ISSUED;
            
            // Set demurrage status accordingly: ISSUED if there is a demurrage fee, otherwise PAID
            if (invoice.demurrageAmount > 0)
            {
                invoice.demurrageStatus = InvoiceStatus.ISSUED;
            }
            else
            {
                invoice.demurrageStatus = InvoiceStatus.PAID;
            }

            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task ProcessFreightPaymentAsync(int invoiceId, int paidByUserId)
        {
            var invoice = await _context.FreightInvoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                throw new KeyNotFoundException("Invoice record not found.");
            }

            // Pay allowed for ISSUED or OVERDUE
            if (invoice.invoiceStatus != InvoiceStatus.ISSUED && invoice.invoiceStatus != InvoiceStatus.OVERDUE)
            {
                throw new InvalidOperationException("Only Issued or Overdue invoices can be paid.");
            }

            invoice.invoiceStatus = InvoiceStatus.PAID;
            invoice.paymentDate = DateTime.UtcNow;
            invoice.paidByUserId = paidByUserId;

            // Complete booking only if both freight and demurrage are paid
            if (invoice.demurrageStatus == InvoiceStatus.PAID || invoice.demurrageAmount == 0m)
            {
                var booking = await _context.ShipmentBookings.FindAsync(invoice.bookingId);
                if (booking != null)
                {
                    booking.bookingStatus = BookingStatus.COMPLETED;
                    _context.Entry(booking).State = EntityState.Modified;
                }
            }

            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task ProcessDemurragePaymentAsync(int invoiceId, int paidByUserId)
        {
            var invoice = await _context.FreightInvoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                throw new KeyNotFoundException("Invoice record not found.");
            }

            // Pay allowed for ISSUED or OVERDUE
            if (invoice.demurrageStatus != InvoiceStatus.ISSUED && invoice.demurrageStatus != InvoiceStatus.OVERDUE)
            {
                throw new InvalidOperationException("Only Issued or Overdue demurrage fees can be paid.");
            }

            invoice.demurrageStatus = InvoiceStatus.PAID;
            invoice.demurragePaymentDate = DateTime.UtcNow;
            invoice.demurragePaidByUserId = paidByUserId;

            // Complete booking only if both freight and demurrage are paid
            if (invoice.invoiceStatus == InvoiceStatus.PAID)
            {
                var booking = await _context.ShipmentBookings.FindAsync(invoice.bookingId);
                if (booking != null)
                {
                    booking.bookingStatus = BookingStatus.COMPLETED;
                    _context.Entry(booking).State = EntityState.Modified;
                }
            }

            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // --- Helper number generator ---

        private async Task<string> GenerateUniqueInvoiceNumberAsync()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            while (true)
            {
                var randStr = new string(Enumerable.Repeat(chars, 4)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                
                var number = $"INV-{DateTime.UtcNow:yyyyMMdd}-{randStr}";
                
                bool exists = await _context.FreightInvoices.AnyAsync(fi => fi.invoiceNumber == number);
                if (!exists)
                {
                    return number;
                }
            }
        }
    }
}
