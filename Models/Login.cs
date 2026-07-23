using System;
using System.ComponentModel.DataAnnotations;
using CargoCaptain.Enums;

namespace CargoCaptain.Models
{
    public class Login
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [StringLength(100)]
        public string AssociatedName { get; set; } = string.Empty;

        // Navigation Properties
        public virtual Employee? Employee { get; set; }
    }
}
