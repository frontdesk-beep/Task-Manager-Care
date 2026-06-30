using System.ComponentModel.DataAnnotations;//data annotations

namespace Task_Manager_Care.Models
{
//2 roles are assigned to users: "Admin" and "Employee".
//Admins have full access to all features, while Employees have limited access based on their assigned tasks and projects.
//The User model includes properties for Id, Name, Email, Password, Role, and CreatedAt to manage user information effectively within the task management system.
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@careinsurance\.ca",
            ErrorMessage = "Email must be a valid email address ending with @careinsurance.ca")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8,
       ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(
       @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
       ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string? Password { get; set; }

        public string Role { get; set; } // "Admin" or "Employee"

        [Required(ErrorMessage = "Created At is required.")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true; // New property to indicate if the user is active or not

        [DataType(DataType.Date)]
        public DateOnly? DOB {  get; set; } // Date of Birth property added to the User model
    }

}
