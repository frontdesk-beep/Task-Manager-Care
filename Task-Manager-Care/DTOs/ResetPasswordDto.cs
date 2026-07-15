using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
