using System.Collections.Generic;
using CargoCaptain.Models;

namespace CargoCaptain.ViewModels
{
    public class PortDashboardViewModel
    {
        public int TotalContainers { get; set; }
        public int GatedInCount { get; set; }
        public int LoadedCount { get; set; }
        public int SailedCount { get; set; }
        public int DischargedCount { get; set; }
        public int GateOutCount { get; set; }

        public List<CargoEvent> RecentMilestones { get; set; } = new List<CargoEvent>();
        public List<Container> AwaitingActionContainers { get; set; } = new List<Container>();
    }
}
