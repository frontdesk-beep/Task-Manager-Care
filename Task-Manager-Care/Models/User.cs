using System.ComponentModel.DataAnnotations;//data annotations

namespace Task_Manager_Care.Models
{
//2 roles are assigned to users: "Admin" and "Employee".
//Admins have full access to all features, while Employees have limited access based on their assigned tasks and projects.
//The User model includes properties for Id, Name, Email, Password, Role, and CreatedAt to manage user information effectively within the task management system.
    public class User
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        //[StringLength(100, MinimumLength = 8)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.")]
        public string? Password { get; set; }

        
        [Required]
        [StringLength(20)]
        public string Role { get; set; } // "Admin" or "Employee"
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true; // New property to indicate if the user is active or not
    }

}
