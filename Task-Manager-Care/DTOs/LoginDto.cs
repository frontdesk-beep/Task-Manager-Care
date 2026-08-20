using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class LoginDto : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (!string.IsNullOrEmpty(Email) &&
                !Email.Trim().EndsWith("@careinsurance.ca", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Email must be a valid email address ending with @careinsurance.ca",
                    new[] { nameof(Email) });
            }
        }
    }
}