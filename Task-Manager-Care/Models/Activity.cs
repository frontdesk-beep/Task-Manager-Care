using System.ComponentModel.DataAnnotations;

namespace Task_Manager_Care.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ChangedById {  get; set; }
        public User? ChangedBy { get;set; }
        public string Action { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Description { get; set; }

    }
}
