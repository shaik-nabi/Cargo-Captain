using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CargoCaptain.Models;
using CargoCaptain.Enums;

namespace CargoCaptain.ViewModels
{
    public class ContainerAllocationViewModel
    {
        [Required]
        public int bookingId { get; set; }

        public string? bookingNumber { get; set; }

        [Required(ErrorMessage = "Container Number is required.")]
        [RegularExpression(@"^[A-Z]{4}\d{7}$", ErrorMessage = "Container number must be in standard prefix format (e.g. MSCU1234567).")]
        [Display(Name = "Container Number")]
        public string containerNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seal Number is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]{6,20}$", ErrorMessage = "Seal Number must be alphanumeric and between 6 and 20 characters.")]
        [Display(Name = "Seal Number")]
        public string sealNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Container Type is required.")]
        [Display(Name = "Container Type")]
        public ContainerType containerType { get; set; }

        [Required(ErrorMessage = "Container Status is required.")]
        [Display(Name = "Container Status")]
        public ContainerStatus containerStatus { get; set; }

        // Grid lists showing existing containers
        public List<Container> AllocatedContainers { get; set; } = new List<Container>();
    }
}
