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
using Task_Manager_Care.Helpers;

namespace Task_Manager_Care.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/comments")]
    [Authorize]
    public class RemarkController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskHub> _hub;

        public RemarkController(AppDbContext context, IHubContext<TaskHub> hub)
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

            var comment = new Remarks
            {
                TaskId = taskId,
                UserId = userId,
                Text = dto.Text,
                CreatedAt = DateTimeHelper.ToEastern(DateTime.UtcNow)
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();
            _context.TaskHistories.Add(new Activity
            {
                TaskId = taskId,
                ChangedById = userId,
                Action = "Comment Added",
                ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                Description = $"{user?.Name} added a comment."
            });

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
            // Decide who should receive notification
            var notifyUserId = task.AssignedToId == userId
                ? task.CreatedById
                : task.AssignedToId;


            // Create notification
            var notification = new Notification
            {
                UserId = notifyUserId,
                TaskId = taskId,
                Message = $"{user?.Name} commented on task '{task.ClientName}'",
                CreatedOn = DateTimeHelper.ToEastern(DateTime.UtcNow)
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Broadcast to subscribed clients
            await _hub.Clients.Group($"task-{taskId}").SendAsync("CommentAdded", result);

            return CreatedAtAction(nameof(GetComments), new { taskId }, result);
        }

    [HttpPut("{commentId}")]
        public async Task<IActionResult> EditComment(int taskId, int commentId, CommentCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == commentId && x.TaskId == taskId);

            if (comment == null)
                return NotFound();

            if (comment.UserId != userId)
                return Forbid();

            comment.Text = dto.Text;

            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"task-{taskId}")
                .SendAsync("CommentEdited", new
                {
                    comment.Id,
                    comment.Text
                });

            return NoContent();
        }
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int taskId, int commentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == commentId && x.TaskId == taskId);

            if (comment == null)
                return NotFound();

            if (comment.UserId != userId)
                return Forbid();

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"task-{taskId}")
                .SendAsync("CommentDeleted", commentId);

            return NoContent();
        }
    }
    public class CommentCreateDto
    {
        public string Text { get; set; } = null!;
    }
}


