using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Task_Manager_Care.Models
{
    //for assigning tasks to each other
    public class TaskItem
    {
        public int Id { get; set; }
        public int? ClientId { get; set; } // Foreign key to Client

        public Client? Client { get; set; } // Navigation property to Client

        [Required]
        [StringLength(100)]
        public string? ClientName { get; set; }


        //2 Categories: "New Client" and "Existing Client".
        //This helps in categorizing tasks based on the client's status, allowing for tailored approaches in task management and client interactions.
        //Navigation property - EF Core to load
        public int ClientCategoryId { get; set; } // Foreign key to ClientCategory
        public ClientCategory? ClientCategory { get; set; }

        [Required(ErrorMessage ="Phone Number is required.")]
        [StringLength(15, ErrorMessage ="Phone number can not exceeds the 15 characters")]
        [Phone(ErrorMessage ="Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public int AssignedToId { get; set; } // User assigned to the task (Employee's name or ID)
        public User? AssignedTo { get; set; } // Navigation property to User

        [Required]
        [DataType(DataType.DateTime)]
        [JsonPropertyName("createdOn")]
        public DateTime Created_On { get; set; } // Date and time when the task was created
        public DateTime? Updated_On { get; set; } // Date and time when the task was last updated (nullable for new tasks)

        public DateTime? CompletedOn { get; set; } // Date and time when the task was completed (nullable for tasks not yet completed)
        public int StatusId { get; set; } // "Pending", "In Progress", "Completed"
        //Navigation property - EF Core to load
        public Status? Status { get; set; } // Navigation property to Status

        [Required]
        [StringLength(500)]
        [JsonPropertyName("task_Description")]
        public string? task_Description { get; set; }
        public string? LongDescription { get; set; } // Additional notes or comments about the task

        [Required(ErrorMessage = "Due Date is required.")]
        [DataType(DataType.Date)]
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
        [Required]
        public int PriorityId { get; set; } // Foreign key to Priority
        public Priority? PriorityNavigation { get; set; } // Navigation property to Priority

        //to check which email sent last- to check evrday email is gonna sent.
        public DateTime? LastOverdueEmailSentAt { get; set; }
        public List<Remarks>? Comments { get; set; } // Navigation property to Comments (one-to-many relationship)
    }
}
