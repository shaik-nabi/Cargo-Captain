using System.Collections.Generic;
using CargoCaptain.Models;

namespace CargoCaptain.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalBookings { get; set; }
        public int TotalContainers { get; set; }
        public int ActiveShipments { get; set; }

        public List<ShipmentBooking> RecentBookings { get; set; } = new List<ShipmentBooking>();
        public List<FreightInvoice> RecentInvoices { get; set; } = new List<FreightInvoice>();

        // Chart.js Vectors
        public List<string> ChartMonths { get; set; } = new List<string>();
        public List<decimal> ChartRevenueData { get; set; } = new List<decimal>();
        public List<decimal> ChartProfitData { get; set; } = new List<decimal>();
    }
}
