using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CargoCaptain.Enums;

namespace CargoCaptain.Models
{
    public class FreightInvoice
    {
        [Key]
        public int invoiceId { get; set; }

        [Required]
        public int bookingId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal freightCharges { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal surchargeAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal demurrageAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal totalAmount { get; set; }

        [Required]
        [StringLength(10)]
        public string currency { get; set; } = string.Empty;

        [Required]
        public InvoiceStatus invoiceStatus { get; set; }

        [Required]
        [StringLength(20)]
        public string invoiceNumber { get; set; } = string.Empty;

        public DateTime? paymentDate { get; set; }

        public int? paidByUserId { get; set; }

        // Demurrage payment tracking
        [Required]
        public InvoiceStatus demurrageStatus { get; set; } = InvoiceStatus.DRAFT;

        public DateTime? demurragePaymentDate { get; set; }

        public int? demurragePaidByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("bookingId")]
        public virtual ShipmentBooking? ShipmentBooking { get; set; }

        [ForeignKey("paidByUserId")]
        public virtual Login? PaidByUser { get; set; }

        [ForeignKey("demurragePaidByUserId")]
        public virtual Login? DemurragePaidByUser { get; set; }
    }
}
