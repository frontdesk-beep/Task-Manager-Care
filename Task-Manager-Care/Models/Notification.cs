namespace Task_Manager_Care.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int? TaskId { get; set; }
        public TaskItem? TaskItem { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
