using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.ViewModels
{
    public class RecordMilestoneViewModel
    {
        [Required]
        public int containerId { get; set; }

        public string? containerNumber { get; set; }

        [Required(ErrorMessage = "Event Type is required.")]
        [Display(Name = "Milestone Event Status")]
        public CargoEventType eventType { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location must be 100 characters or less.")]
        [Display(Name = "Current Port Location")]
        public string eventLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Record Date")]
        public DateTime eventDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Time is required.")]
        [DataType(DataType.Time)]
        [Display(Name = "Event Record Time")]
        public TimeSpan eventTime { get; set; } = DateTime.Now.TimeOfDay;

        [StringLength(500, ErrorMessage = "Remarks must be 500 characters or less.")]
        [Display(Name = "Operation Remarks")]
        public string remarks { get; set; } = string.Empty;

        // Displays history logs on same page
        public List<CargoEvent> EventHistory { get; set; } = new List<CargoEvent>();
    }
}
