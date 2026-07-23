using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;

namespace CargoCaptain.Interfaces
{
    public interface IReportsService
    {
        Task<IEnumerable<ShipmentReportRow>> GetShipmentReportAsync(DateTime? start, DateTime? end);
        
        Task<IEnumerable<FreightInvoice>> GetInvoiceReportAsync(InvoiceStatus? status);
        
        Task<RevenueReportSummary> GetRevenueReportAsync(DateTime? start, DateTime? end);
        
        Task<IEnumerable<DemurrageReportRow>> GetDemurrageReportAsync();
        
        Task<IEnumerable<BookingReportRow>> GetBookingReportAsync();
        
        Task<IEnumerable<ContainerReportRow>> GetContainerReportAsync();
    }
}
