namespace Task_Manager_Care.DTOs
{
    public class RecentTaskDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string AssignedTo { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
