using System.ComponentModel.DataAnnotations;

namespace CargoCaptain.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public string PaymentType { get; set; } = string.Empty; // "Freight" or "Demurrage"

        [Required(ErrorMessage = "Cardholder Name is required.")]
        [Display(Name = "Cardholder Name")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card Number is required.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card Number must be exactly 16 numeric digits.")]
        [Display(Name = "Credit Card Number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiration Date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiration Date must be in MM/YY format.")]
        [Display(Name = "Expiration Date (MM/YY)")]
        public string ExpirationDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 numeric digits.")]
        [Display(Name = "Security Code (CVV)")]
        public string CVV { get; set; } = string.Empty;

        [Display(Name = "Total Invoice Amount ($ USD)")]
        public decimal TotalAmount { get; set; }
    }
}
