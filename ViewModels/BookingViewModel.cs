using System;
using System.ComponentModel.DataAnnotations;

namespace CargoCaptain.ViewModels
{
    public class BookingViewModel
    {
        public int bookingId { get; set; }

        [Required(ErrorMessage = "Consignee Name is required.")]
        [StringLength(100, ErrorMessage = "Consignee Name cannot exceed 100 characters.")]
        [Display(Name = "Consignee Name")]
        public string consigneeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Origin Port is required.")]
        [StringLength(100, ErrorMessage = "Origin Port cannot exceed 100 characters.")]
        [Display(Name = "Origin Port")]
        public string originPort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination Port is required.")]
        [StringLength(100, ErrorMessage = "Destination Port cannot exceed 100 characters.")]
        [Display(Name = "Destination Port")]
        public string destinationPort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cargo Weight is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Cargo weight must be greater than zero.")]
        [Display(Name = "Cargo Weight (Metric Tons)")]
        public decimal cargoWeight { get; set; }

        [Required(ErrorMessage = "Cargo Description is required.")]
        [StringLength(500, ErrorMessage = "Cargo description cannot exceed 500 characters.")]
        [Display(Name = "Cargo Description")]
        public string cargoDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Booking/Departure Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime bookingDate { get; set; } = DateTime.Today;
    }
}
