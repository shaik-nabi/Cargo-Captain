using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoCaptain.Models
{
    public class Employee
    {
        [Key]
        public int employeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string firstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string lastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string phoneNumber { get; set; } = string.Empty;

        [Required]
        public int userId { get; set; }

        // Navigation Properties
        [ForeignKey("userId")]
        public virtual Login? Login { get; set; }
    }
}
