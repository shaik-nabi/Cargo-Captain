using System.Collections.Generic;
using CargoCaptain.Models;

namespace CargoCaptain.ViewModels
{
    public class ForwarderDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int PendingAllocationsCount { get; set; }
        public int CompletedAllocationsCount { get; set; }
        public int TotalContainers { get; set; }

        public List<ShipmentBooking> RecentBookings { get; set; } = new List<ShipmentBooking>();
        
        // Custom widget: Recent container allocations
        public List<Container> RecentAllocations { get; set; } = new List<Container>();
    }
}
