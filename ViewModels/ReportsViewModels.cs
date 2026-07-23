using System;

namespace CargoCaptain.ViewModels
{
    public class ShipmentReportRow
    {
        public string BookingNumber { get; set; } = string.Empty;
        public string Shipper { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string LatestMilestone { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    public class RevenueReportSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal PaidRevenue { get; set; }
        public decimal OutstandingRevenue { get; set; }
        
        public decimal OceanFreightTotal { get; set; }
        public decimal SurchargeTotal { get; set; }
        public decimal DemurrageTotal { get; set; }

        public int DraftCount { get; set; }
        public int IssuedCount { get; set; }
        public int PaidCount { get; set; }
    }

    public class DemurrageReportRow
    {
        public string ContainerNumber { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public int TotalDays { get; set; }
        public int ChargeableDays { get; set; }
        public decimal DemurrageAmount { get; set; }
    }

    public class BookingReportRow
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalWeight { get; set; }
    }

    public class ContainerReportRow
    {
        public string ContainerNumber { get; set; } = string.Empty;
        public string ContainerType { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string BookingNumber { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string LatestMilestone { get; set; } = string.Empty;
    }
}
