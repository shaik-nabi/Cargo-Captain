using System;

namespace CargoCaptain.ViewModels
{
    public class TrackingTimelineItem
    {
        public string EventName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
