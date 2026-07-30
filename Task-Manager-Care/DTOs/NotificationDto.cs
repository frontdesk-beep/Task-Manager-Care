namespace Task_Manager_Care.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } 
        public string Message { get; set; }
        public string Type { get; set; }
        public int? TaskId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedByName { get; set; }
}
}
