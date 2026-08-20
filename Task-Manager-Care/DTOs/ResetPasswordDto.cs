using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}