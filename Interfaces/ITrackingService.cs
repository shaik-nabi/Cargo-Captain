using System.Threading.Tasks;
using CargoCaptain.ViewModels;

namespace CargoCaptain.Interfaces
{
    public interface ITrackingService
    {
        Task<TrackingDetailsViewModel?> GetTrackingDetailsAsync(string bookingNumber);
    }
}
