using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClientName { get; set; } = string.Empty!;

        [Range(1, int.MaxValue, ErrorMessage = "ClientCategoryId must be greater than 0.")]
        public int ClientCategoryId { get; set; }
        public ClientCategory? ClientCategory { get; set; }

        [StringLength(100)]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15)]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage ="Email is required.")]
        [EmailAddress]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? Address { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedById { get; set; }
        public User? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
