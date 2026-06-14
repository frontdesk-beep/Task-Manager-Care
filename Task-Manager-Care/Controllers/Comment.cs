using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    internal class Comment : CommentEntity
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}