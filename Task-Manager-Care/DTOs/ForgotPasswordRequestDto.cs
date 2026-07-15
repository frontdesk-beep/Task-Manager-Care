using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        //[RegularExpression(@"[A-Za-z0-9._%+-]+@careinsurance.ca")]
        public string Email { get; set; }

    }
}
