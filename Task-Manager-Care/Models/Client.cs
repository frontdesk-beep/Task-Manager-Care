using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClientName { get; set; } = string.Empty!;

        public int ClientCategoryId { get; set; }
        public ClientCategory? ClientCategory { get; set; }

        [StringLength(100)]
        public string? CompanyName { get; set; }

        [StringLength(100)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? Address { get; set; }

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }
            public User? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
