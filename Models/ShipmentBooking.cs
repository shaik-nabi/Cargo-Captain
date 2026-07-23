using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargoCaptain.Enums;

namespace CargoCaptain.Models
{
    public class ShipmentBooking
    {
        [Key]
        public int bookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string bookingNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string shipperName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string consigneeName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string originPort { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string destinationPort { get; set; } = string.Empty;

        [Required]
        public BookingStatus bookingStatus { get; set; }

        [Required]
        public decimal cargoWeight { get; set; }

        [Required]
        [StringLength(500)]
        public string cargoDescription { get; set; } = string.Empty;

        [Required]
        public DateTime bookingDate { get; set; }

        [Required]
        public int userId { get; set; }

        // Navigation Properties
        public virtual Login? Login { get; set; }
        public virtual ICollection<Container> Containers { get; set; } = new List<Container>();
        
        // 1-to-1 Navigation Properties
        public virtual CustomsDeclaration? CustomsDeclaration { get; set; }
        public virtual FreightInvoice? FreightInvoice { get; set; }
    }
}
