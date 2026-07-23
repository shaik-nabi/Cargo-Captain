using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.Interfaces;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;

namespace CargoCaptain.Services
{
    public class ReportsService : IReportsService
    {
        private readonly ApplicationDbContext _context;

        public ReportsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShipmentReportRow>> GetShipmentReportAsync(DateTime? start, DateTime? end)
        {
            var query = _context.ShipmentBookings
                .Include(sb => sb.Containers)
                    .ThenInclude(c => c.CargoEvents)
                .AsQueryable();

            if (start.HasValue)
            {
                query = query.Where(sb => sb.bookingDate >= start.Value);
            }
            if (end.HasValue)
            {
                query = query.Where(sb => sb.bookingDate <= end.Value);
            }

            var bookings = await query.OrderByDescending(sb => sb.bookingId).ToListAsync();
            var rows = new List<ShipmentReportRow>();

            foreach (var b in bookings)
            {
                var latestEvent = b.Containers
                    .SelectMany(c => c.CargoEvents ?? Enumerable.Empty<CargoEvent>())
                    .OrderBy(e => e.eventTimestamp)
                    .LastOrDefault();

                rows.Add(new ShipmentReportRow
                {
                    BookingNumber = b.bookingNumber,
                    Shipper = b.shipperName,
                    Origin = b.originPort,
                    Destination = b.destinationPort,
                    CurrentStatus = b.bookingStatus.ToString(),
                    LatestMilestone = latestEvent != null ? latestEvent.eventType.ToString() : "Awaiting Allocation",
                    BookingDate = b.bookingDate
                });
            }

            return rows;
        }

        public async Task<IEnumerable<FreightInvoice>> GetInvoiceReportAsync(InvoiceStatus? status)
        {
            var query = _context.FreightInvoices
                .Include(fi => fi.ShipmentBooking)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(fi => fi.invoiceStatus == status.Value);
            }

            return await query.OrderByDescending(fi => fi.invoiceId).ToListAsync();
        }

        public async Task<RevenueReportSummary> GetRevenueReportAsync(DateTime? start, DateTime? end)
        {
            var query = _context.FreightInvoices.AsQueryable();

            if (start.HasValue)
            {
                query = query.Where(fi => fi.paymentDate >= start.Value || (fi.paymentDate == null && fi.invoiceId > 0));
            }
            if (end.HasValue)
            {
                query = query.Where(fi => fi.paymentDate <= end.Value || (fi.paymentDate == null && fi.invoiceId > 0));
            }

            var invoices = await query.ToListAsync();

            var total = invoices.Sum(i => i.totalAmount);
            var paid = invoices.Where(i => i.invoiceStatus == InvoiceStatus.PAID).Sum(i => i.totalAmount);
            var outstanding = invoices.Where(i => i.invoiceStatus == InvoiceStatus.ISSUED || i.invoiceStatus == InvoiceStatus.OVERDUE).Sum(i => i.totalAmount);

            var freight = invoices.Sum(i => i.freightCharges);
            var surcharges = invoices.Sum(i => i.surchargeAmount);
            var demurrage = invoices.Sum(i => i.demurrageAmount);

            return new RevenueReportSummary
            {
                TotalRevenue = total,
                PaidRevenue = paid,
                OutstandingRevenue = outstanding,
                OceanFreightTotal = freight,
                SurchargeTotal = surcharges,
                DemurrageTotal = demurrage,
                DraftCount = invoices.Count(i => i.invoiceStatus == InvoiceStatus.DRAFT),
                IssuedCount = invoices.Count(i => i.invoiceStatus == InvoiceStatus.ISSUED),
                PaidCount = invoices.Count(i => i.invoiceStatus == InvoiceStatus.PAID)
            };
        }

        public async Task<IEnumerable<DemurrageReportRow>> GetDemurrageReportAsync()
        {
            var containers = await _context.Containers
                .Include(c => c.CargoEvents)
                .Include(c => c.ShipmentBooking)
                    .ThenInclude(sb => sb!.CustomsDeclaration)
                .ToListAsync();

            var rows = new List<DemurrageReportRow>();

            foreach (var c in containers)
            {
                if (c.CargoEvents == null || !c.CargoEvents.Any()) continue;

                var booking = c.ShipmentBooking;
                if (booking == null) continue;

                var customs = booking.CustomsDeclaration;
                var decType = customs?.declarationType ?? DeclarationType.EXPORT;

                double days = 0;
                DateTime? arrival = null;
                DateTime? departure = null;

                if (decType == DeclarationType.EXPORT)
                {
                    var gateInEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.GATE_IN);
                    var sailedEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.SAILED);
                    if (gateInEvent != null) arrival = gateInEvent.eventTimestamp;
                    if (sailedEvent != null) departure = sailedEvent.eventTimestamp;
                }
                else // IMPORT
                {
                    var dischargedEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.DISCHARGED);
                    var gateOutEvent = c.CargoEvents.FirstOrDefault(e => e.eventType == CargoEventType.GATE_OUT);
                    if (dischargedEvent != null) arrival = dischargedEvent.eventTimestamp;
                    if (gateOutEvent != null) departure = gateOutEvent.eventTimestamp;
                }

                if (arrival.HasValue && departure.HasValue)
                {
                    days = (departure.Value - arrival.Value).TotalDays;
                }

                int totalDays = Math.Max(0, (int)Math.Ceiling(days));
                int chargeableDays = Math.Max(0, totalDays - 3);

                rows.Add(new DemurrageReportRow
                {
                    ContainerNumber = c.containerNumber,
                    BookingNumber = booking.bookingNumber,
                    ArrivalDate = arrival,
                    DepartureDate = departure,
                    TotalDays = totalDays,
                    ChargeableDays = chargeableDays,
                    DemurrageAmount = chargeableDays * 50m
                });
            }

            return rows;
        }

        public async Task<IEnumerable<BookingReportRow>> GetBookingReportAsync()
        {
            var groups = await _context.ShipmentBookings
                .GroupBy(sb => sb.bookingStatus)
                .Select(g => new BookingReportRow
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalWeight = g.Sum(sb => sb.cargoWeight)
                })
                .ToListAsync();

            return groups;
        }

        public async Task<IEnumerable<ContainerReportRow>> GetContainerReportAsync()
        {
            var containers = await _context.Containers
                .Include(c => c.CargoEvents)
                .Include(c => c.ShipmentBooking)
                .OrderByDescending(c => c.containerId)
                .ToListAsync();

            var rows = new List<ContainerReportRow>();

            foreach (var c in containers)
            {
                var latestEvent = c.CargoEvents?.OrderBy(e => e.eventTimestamp).LastOrDefault();

                rows.Add(new ContainerReportRow
                {
                    ContainerNumber = c.containerNumber,
                    ContainerType = c.containerType.ToString(),
                    CurrentStatus = c.containerStatus.ToString(),
                    BookingNumber = c.ShipmentBooking?.bookingNumber ?? "N/A",
                    CurrentLocation = latestEvent != null ? latestEvent.eventLocation : "Origin Staging Yard",
                    LatestMilestone = latestEvent != null ? latestEvent.eventType.ToString() : "Allocation Finished"
                });
            }

            return rows;
        }
    }
}
