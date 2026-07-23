using System.ComponentModel.DataAnnotations;
using CargoCaptain.Enums;

namespace CargoCaptain.ViewModels
{
    public class FileDeclarationViewModel
    {
        [Required]
        public int bookingId { get; set; }

        public string? bookingNumber { get; set; }

        [Required(ErrorMessage = "HS Code is required.")]
        [RegularExpression(@"^\d{6,10}$", ErrorMessage = "HS Code must be a numeric string between 6 and 10 digits.")]
        [Display(Name = "HS Code (Harmonized System)")]
        public string hsCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Declared Value is required.")]
        [Range(0.01, 9999999999.99, ErrorMessage = "Declared value must be greater than zero.")]
        [Display(Name = "Declared Cargo Value ($ USD)")]
        public decimal declaredValue { get; set; }

        [Required(ErrorMessage = "Declaration Type is required.")]
        [Display(Name = "Declaration Type")]
        public DeclarationType declarationType { get; set; }
    }
}
