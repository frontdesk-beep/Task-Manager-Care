namespace Task_Manager_Care.Models
{
//2 roles are assigned to users: "Admin" and "Employee".
//Admins have full access to all features, while Employees have limited access based on their assigned tasks and projects.
//The User model includes properties for Id, Name, Email, Password, Role, and CreatedAt to manage user information effectively within the task management system.
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Admin" or "Employee"
        public DateTime CreatedAt { get; set; }
    }

}
