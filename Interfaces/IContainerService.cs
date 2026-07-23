using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;

namespace CargoCaptain.Interfaces
{
    public interface IContainerService
    {
        Task<IEnumerable<ShipmentBooking>> GetAllBookingsAsync();
        
        Task<IEnumerable<ShipmentBooking>> GetPendingAllocationsAsync();
        
        Task<IEnumerable<ShipmentBooking>> GetCompletedAllocationsAsync();
        
        Task<IEnumerable<Container>> GetContainersByBookingIdAsync(int bookingId);
        
        Task<IEnumerable<Container>> GetRecentAllocationsAsync(int count);
        
        Task AllocateContainerAsync(Container container);
        
        Task RemoveContainerAsync(int containerId);
        
        Task<bool> ContainerNumberExistsAsync(string containerNumber, int? excludeContainerId = null);
        
        Task<Container?> GetContainerByIdAsync(int containerId);
        
        Task<ShipmentBooking?> GetBookingByIdAsync(int bookingId);
    }
}
