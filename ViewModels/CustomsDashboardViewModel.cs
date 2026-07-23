using System;
using System.Collections.Generic;
using CargoCaptain.Models;
using CargoCaptain.Controllers;
using CargoCaptain.Enums;

namespace CargoCaptain.ViewModels
{
    public class CustomsEntryViewModel
    {
        public bool IsFiledDeclaration { get; set; }
        public int? DeclarationId { get; set; }
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string ShipperName { get; set; } = string.Empty;
        public string ConsigneeName { get; set; } = string.Empty;
        public string OriginPort { get; set; } = string.Empty;
        public string DestinationPort { get; set; } = string.Empty;
        public int ContainerCount { get; set; }
        public List<DocumentMetadata> Documents { get; set; } = new List<DocumentMetadata>();
        public DeclarationType? DeclarationType { get; set; }
        public string? HsCode { get; set; }
        public decimal DeclaredValue { get; set; }
        public decimal CalculatedDuty { get; set; }
        public ClearanceStatus? ClearanceStatus { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CustomsDashboardViewModel
    {
        public int TotalSubmissions { get; set; }
        public int PendingActionCount { get; set; }
        public int ApprovedCount { get; set; }
        public int HeldCount { get; set; }

        public List<CustomsEntryViewModel> RecentEntries { get; set; } = new List<CustomsEntryViewModel>();
    }
}
