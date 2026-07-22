using Task_Manager_Care.Data;
using Task_Manager_Care.Hubs;
using Task_Manager_Care.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class TaskNotificationService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<TaskHub> _hub;

        public TaskNotificationService(AppDbContext db, IHubContext<TaskHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task PublishTaskUpdatedAsync(TaskItem task)
        {
            var payload = new
            {
                type = "task-updated",
                taskId = task.Id,
                task = new
                {
                    task.Id,
                    task.ClientName,
                    task.task_Description,
                    task.LongDescription,
                    task.AssignedToId,
                    task.CreatedById,
                    task.StatusId,
                    task.PriorityId,
                    task.DueDate,
                    task.Updated_On
                }
            };

            var recipients = new[] { task.AssignedToId, task.CreatedById }.Distinct();
            foreach (var userId in recipients)
            {
                await _hub.Clients.Group($"user-{userId}").SendAsync("ReceiveTaskUpdate", payload);
            }
        }

        public async Task PublishCommentAddedAsync(TaskItem task, Remarks comment)
        {
            var commentPayload = new
            {
                type = "comment-added",
                taskId = task.Id,
                comment = new
                {
                    comment.Id,
                    comment.TaskId,
                    comment.UserId,
                    authorName = comment.User?.Name,
                    text = comment.Text,
                    createdAt = comment.CreatedAt
                }
            };

            await _hub.Clients.Group($"task-{task.Id}").SendAsync("ReceiveComment", commentPayload);

            var recipients = new[] { task.AssignedToId, task.CreatedById }.Distinct();
            foreach (var userId in recipients)
            {
                await _hub.Clients.Group($"user-{userId}").SendAsync("ReceiveTaskUpdate", commentPayload);
            }
        }
    }
}