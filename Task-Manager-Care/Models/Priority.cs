using System.ComponentModel.DataAnnotations;//data annotations

namespace Task_Manager_Care.Models
{
    public class Priority
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } // "High", "Medium", "Low"
    }
}
