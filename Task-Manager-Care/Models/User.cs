namespace Task_Manager_Care.Models
{
    // 2 roles are assigned to users: "Admin" and "Employee".
    // Admins have full access to all features, while Employees have limited access based on their assigned tasks and projects.
    // The User model includes properties for Id, Name, Email, Password, Role, and CreatedAt to manage user information effectively within the task management system.
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Stores a hashed password (via PasswordHasher<User>) — never validate this
        // with a complexity/length regex, that belongs on the request DTOs instead.
        public string? Password { get; set; }

        public string Role { get; set; } = "Employee"; // "Admin" or "Employee"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // indicates if the user is active

        public DateOnly? DOB { get; set; } // Date of Birth

        // for the email reset-password link
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }
}