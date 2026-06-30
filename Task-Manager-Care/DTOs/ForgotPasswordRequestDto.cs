using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@careinsurance.ca")]
        public string Email { get; set; }


        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; }
    }
}
