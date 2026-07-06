namespace Task_Manager_Care.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? TaskId { get; set; }
    }
}
