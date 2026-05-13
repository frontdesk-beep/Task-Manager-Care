using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Task_Manager_Care.Models
{
    //for assigning tasks to each other
    public class TaskItem
    {
        public int Id { get; set; }
        public string ClientName { get; set; }


        //2 Categories: "New Client" and "Existing Client".
        //This helps in categorizing tasks based on the client's status, allowing for tailored approaches in task management and client interactions.
        public int ClientId { get; set; } // Foreign key to User (Client)   
        //Navigation property - EF Core to load
        public ClientCategory ClientCategory { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string AssignedTo { get; set; } // User assigned to the task (Employee's name or ID)
        public string Created_On { get; set; } // Date and time when the task was created

        public int StatusId { get; set; } // "Pending", "In Progress", "Completed"
        //Navigation property - EF Core to load
        public Status Status { get; set; } // Navigation property to Status

        public string task_Description { get; set; }

        public string DueDate { get; set; } // Date by which the task should be completed
        public string CreatedBy { get; set; } // User who created the task (name)

        //these 3 of them are for the service category, which is a foreign key to the ServiceCategory model,
        //allowing us to categorize tasks based on the type of service they are related to.
        public int ServiceCategoryId { get; set; } // Foreign key to ServiceCategory

        //Navigation property - EF Core to load
        public ServiceCategory ServiceCategory { get; set; } // Navigation property to ServiceCategory
    }
}
