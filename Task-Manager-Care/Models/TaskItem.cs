using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;


namespace Task_Manager_Care.Models
{
    //for assigning tasks to each other
    public class TaskItem
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string ClientName { get; set; }


        //2 Categories: "New Client" and "Existing Client".
        //This helps in categorizing tasks based on the client's status, allowing for tailored approaches in task management and client interactions.
        //Navigation property - EF Core to load
        public int ClientCategoryId { get; set; } // Foreign key to ClientCategory
        public ClientCategory? ClientCategory { get; set; }

        [Required]
        [StringLength(100)]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int AssignedToId { get; set; } // User assigned to the task (Employee's name or ID)
        public User? AssignedTo { get; set; } // Navigation property to User

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created_On { get; set; } // Date and time when the task was created

        public int StatusId { get; set; } // "Pending", "In Progress", "Completed"
        //Navigation property - EF Core to load
        public Status? Status { get; set; } // Navigation property to Status

        [Required]
        [StringLength(500)]
        public string task_Description { get; set; }

        [Required]
        public string Priority { get; set; }

        [Required]
        public DateTime DueDate { get; set; } // Date by which the task should be completed
        
        [Required]
        public int CreatedById { get; set; } // User who created the task (name)
        public User? CreatedBy { get; set; } // Navigation property to User who created the task

        //these 3 of them are for the service category, which is a foreign key to the ServiceCategory model,
        //allowing us to categorize tasks based on the type of service they are related to.
        public int ServiceCategoryId { get; set; } // Foreign key to ServiceCategory

        //Navigation property - EF Core to load - ? - for accepting nullable values also
        public ServiceCategory? ServiceCategory { get; set; } // Navigation property to ServiceCategory

        //Priorities
        public int PriorityId { get; set; } // Foreign key to Priority
        public Priority? PriorityNavigation { get; set; } // Navigation property to Priority
    }
}
