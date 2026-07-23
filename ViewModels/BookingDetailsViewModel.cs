using System.Collections.Generic;
using CargoCaptain.Models;
using CargoCaptain.Controllers;

namespace CargoCaptain.ViewModels
{
    public class BookingDetailsViewModel
    {
        public ShipmentBooking Booking { get; set; } = null!;
        public List<Container> Containers { get; set; } = new List<Container>();
        public List<CargoEvent> TrackingEvents { get; set; } = new List<CargoEvent>();
        public List<DocumentMetadata> UploadedDocuments { get; set; } = new List<DocumentMetadata>();
        public FreightInvoice? Invoice { get; set; }
    }
}
