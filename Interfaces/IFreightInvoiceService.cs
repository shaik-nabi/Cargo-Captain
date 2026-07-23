using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;

namespace CargoCaptain.Interfaces
{
    public interface IFreightInvoiceService
    {
        Task<IEnumerable<FreightInvoice>> GetAllInvoicesAsync();
        
        Task<IEnumerable<FreightInvoice>> GetInvoicesByShipperIdAsync(int shipperUserId);
        
        Task<FreightInvoice?> GetInvoiceByIdAsync(int id);
        
        Task<FreightInvoice?> GetInvoiceByBookingIdAsync(int bookingId);
        
        Task<FreightInvoice> GenerateInvoiceAsync(int bookingId);
        
        Task IssueInvoiceAsync(int invoiceId);
        Task ProcessFreightPaymentAsync(int invoiceId, int paidByUserId);
        Task ProcessDemurragePaymentAsync(int invoiceId, int paidByUserId);
    }
}
