using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;

namespace CargoCaptain.Interfaces
{
    public interface IShipmentBookingService
    {
        Task<IEnumerable<ShipmentBooking>> GetShipperBookingsAsync(int userId);
        
        Task<ShipmentBooking?> GetBookingByIdAsync(int id);
        
        Task CreateBookingAsync(ShipmentBooking booking, int userId);
        
        Task UpdateBookingAsync(ShipmentBooking booking, int userId);
        
        Task CancelBookingAsync(int id, int userId);
        
        Task<bool> BookingNumberExistsAsync(string bookingNumber);
        
        Task<IEnumerable<CargoEvent>> GetBookingTrackingEventsAsync(int bookingId);
        
        Task<FreightInvoice?> GetBookingInvoiceAsync(int bookingId);
    }
}
