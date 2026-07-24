namespace Task_Manager_Care.Models
{
    public class Remarks
    {
        public int Id { get; set; } // Primary key for the comment
        public int TaskId { get; set; } // Foreign key to the associated task
        public TaskItem? TaskItem { get; set; } // Navigation property to the associated task
        public int UserId { get; set; } // Foreign key to the user who made the comment
        public User? User { get; set; } // Navigation property to the user who made the comment
        public string Text { get; set; } // The content of the comment
        public DateTime CreatedAt { get; set; } // Timestamp for when the comment was created
    }
}
