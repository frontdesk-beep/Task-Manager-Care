namespace Task_Manager_Care.Models
{
    public class Notification
    {
        public int Id { get; set; }
        //receiver
        public int? UserId { get; set; }
        public User? User { get; set; }

        // person who did action
        public int? CreatedById { get; set; }
        public int? TaskId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public string Type { get; set; } = string.Empty;
        // TaskAssigned, TaskUpdated, CommentAdded
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }


    }
}
