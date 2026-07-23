using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CargoCaptain.Enums;

namespace CargoCaptain.Models
{
    public class CustomsDeclaration
    {
        [Key]
        public int declarationId { get; set; }

        [Required]
        public int bookingId { get; set; }

        [Required]
        [StringLength(10)]
        public string hsCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal declaredValue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal calculatedDuty { get; set; }

        [Required]
        public ClearanceStatus clearanceStatus { get; set; }

        [Required]
        public DeclarationType declarationType { get; set; }

        // Navigation Properties
        [ForeignKey("bookingId")]
        public virtual ShipmentBooking? ShipmentBooking { get; set; }
    }
}
