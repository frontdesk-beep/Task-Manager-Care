using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.DTOs
{
    public class UpdateProfileRequest
    {
        [Required]
        public string Name { get; set; }    
        public string? CurrentPassword {  get; set; }
        public string? NewPassword { get; set; }
    }
}
