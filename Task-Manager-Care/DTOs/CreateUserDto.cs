using System.ComponentModel.DataAnnotations;
using Task_Manager_Care.Validation;

namespace Task_Manager_Care.DTOs
{
    public class CreateUserDto
    {
        //2 roles are assigned to users: "Admin" and "Employee".
        //Admins have full access to all features, while Employees have limited access based on their assigned tasks and projects.
        //The User model includes properties for Id, Name, Email, Password, Role, and CreatedAt to manage user information effectively within the task management system
            public int Id { get; set; }

            //Name
            [Required(ErrorMessage = "Name is required.")]
            [StringLength(50, MinimumLength = 2,
                ErrorMessage = "Name must be between 2 and 50 characters.")]
            [RegularExpression(@"^[A-Za-z]+(?:[\s'-][A-Za-z]+)*$",
                ErrorMessage = "Name can only contain letters, spaces, hyphens and apostrophes.")]
            public string Name { get; set; }

            //Email
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid Email Format.")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
            [RegularExpression(@"^[A-Za-z0-9._%+-]+@careinsurance\.ca$",
            ErrorMessage ="Email must end with @careinsurance.ca")]
            public string Email { get; set; }

            //Password
            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters.")]
            [RegularExpression(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
                ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
            public string? Password { get; set; }


            //Role
            [Required(ErrorMessage = "Role is required.")]
            [StringLength(20)]
            public string Role { get; set; } // "Admin" or "Employee"

            //Created Date 
            [Required(ErrorMessage = "Created Date is required.")]
            [DataType(DataType.Date)]
            [NotInFuture(ErrorMessage="JoiningDate cannot be in the future.")]
            public DateTime CreatedAt { get; set; }

            [DataType(DataType.Date)]
            public DateOnly? DOB { get; set; }
            public bool IsActive { get; set; } = true; // New property to indicate if the user is active or not
        }

    }


