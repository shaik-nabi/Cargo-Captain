using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CargoCaptain.Enums;

namespace CargoCaptain.Models
{
    public class CargoEvent
    {
        [Key]
        public int eventId { get; set; }

        [Required]
        public int containerId { get; set; }

        [Required]
        public CargoEventType eventType { get; set; }

        [Required]
        [StringLength(100)]
        public string eventLocation { get; set; } = string.Empty;

        [Required]
        public DateTime eventTimestamp { get; set; }

        [StringLength(500)]
        public string remarks { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string recordedBy { get; set; } = string.Empty;

        [Required]
        public DateTime createdDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("containerId")]
        public virtual Container? Container { get; set; }
    }
}
