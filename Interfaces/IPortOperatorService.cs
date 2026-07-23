using System.Collections.Generic;
using System.Threading.Tasks;
using CargoCaptain.Models;

namespace CargoCaptain.Interfaces
{
    public interface IPortOperatorService
    {
        Task<IEnumerable<Container>> GetAllContainersAsync();
        
        Task<IEnumerable<Container>> GetContainersBySectionAsync(string section);
        
        Task<Container?> GetContainerByIdAsync(int containerId);
        
        Task<IEnumerable<CargoEvent>> GetRecentEventsAsync(int count);
        
        Task<IEnumerable<CargoEvent>> GetEventsByContainerIdAsync(int containerId);
        
        Task RecordCargoEventAsync(CargoEvent cargoEvent, string recordedBy);
        
        Task<IEnumerable<Container>> GetContainersAwaitingActionAsync();
    }
}
