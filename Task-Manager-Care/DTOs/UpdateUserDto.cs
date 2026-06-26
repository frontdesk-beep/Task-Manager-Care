using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@careinsurance\.ca",
            ErrorMessage = "Email must be a valid email address ending with @careinsurance.ca")]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; } // "Admin" or "Employee"

    }
}
