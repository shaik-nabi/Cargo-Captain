using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CargoCaptain.Enums;

namespace CargoCaptain.Models
{
    public class Container
    {
        [Key]
        public int containerId { get; set; }

        [Required]
        [StringLength(20)]
        public string containerNumber { get; set; } = string.Empty;

        [Required]
        public ContainerType containerType { get; set; }

        [Required]
        public int bookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string sealNumber { get; set; } = string.Empty;

        [Required]
        public ContainerStatus containerStatus { get; set; }

        // Navigation Properties
        [ForeignKey("bookingId")]
        public virtual ShipmentBooking? ShipmentBooking { get; set; }

        public virtual ICollection<CargoEvent> CargoEvents { get; set; } = new List<CargoEvent>();
    }
}
