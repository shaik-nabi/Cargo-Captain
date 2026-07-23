using System.ComponentModel.DataAnnotations;
using CargoCaptain.Enums;

namespace CargoCaptain.ViewModels
{
    public class EmployeeViewModel
    {
        public int employeeId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string firstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string lastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string phoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [Display(Name = "Account Username")]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [Display(Name = "Account Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please assign a system role.")]
        [Display(Name = "Account Role")]
        public UserRole Role { get; set; }
    }
}
