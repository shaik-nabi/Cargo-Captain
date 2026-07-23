using System;
using System.Collections.Generic;

namespace CargoCaptain.ViewModels
{
    public class TrackingDetailsViewModel
    {
        public string BookingNumber { get; set; } = string.Empty;
        public string OriginPort { get; set; } = string.Empty;
        public string DestinationPort { get; set; } = string.Empty;
        public string CargoDescription { get; set; } = string.Empty;
        
        public string CurrentStatus { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public DateTime? LatestUpdate { get; set; }

        public List<TrackingTimelineItem> TimelineItems { get; set; } = new List<TrackingTimelineItem>();
    }
}
