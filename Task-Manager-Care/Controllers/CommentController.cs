using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Task_Manager_Care.Controllers;
using Task_Manager_Care.Data;
using Task_Manager_Care.Hubs;
using Task_Manager_Care.Models;
using Microsoft.AspNetCore.Authorization;  

namespace Task_Manager_Care.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskHub> _hub;

        public CommentsController(AppDbContext context, IHubContext<TaskHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var result = comments.Select(c => new
            {
                c.Id,
                c.TaskId,
                c.UserId,
                authorName = c.User?.Name,
                text = c.Text,
                createdAt = c.CreatedAt
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int taskId, [FromBody] CommentCreateDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound("Task not found");

            var userId = int.Parse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = userId,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var result = new
            {
                comment.Id,
                comment.TaskId,
                comment.UserId,
                authorName = user?.Name,
                text = comment.Text,
                createdAt = comment.CreatedAt
            };

            // Create notification
            var notification = new Notification
            {
                UserId = task.CreatedById,
                TaskId = taskId,
                Message = $"{user?.Name} commented on task '{task.ClientName}'",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Broadcast to subscribed clients
            await _hub.Clients.Group($"task-{taskId}").SendAsync("CommentAdded", result);

            return CreatedAtAction(nameof(GetComments), new { taskId }, result);
        }
    }
    public class CommentCreateDto
    {
        public string Text { get; set; } = null!;
    }
}


