using System.Collections.Generic;
using CargoCaptain.Models;

namespace CargoCaptain.ViewModels
{
    public class ShipperDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }

        public List<ShipmentBooking> RecentBookings { get; set; } = new List<ShipmentBooking>();
        
        // Dynamic logs for Recent Activities
        public List<string> RecentActivities { get; set; } = new List<string>();
    }
}
