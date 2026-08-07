using Task_Manager_Care.Hubs;
using Task_Manager_Care.Models;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Services
{
    public class TaskNotificationService
    {
        private readonly IHubContext<TaskHub> _hub;

        public TaskNotificationService(
            IHubContext<TaskHub> hub)
        {
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
                    task.AssignedToId,
                    task.CreatedById,
                    task.StatusId,
                    task.PriorityId,
                    task.DueDate
                }
            };


            var recipients = new[]
            {
                task.AssignedToId,
                task.CreatedById
            }
            .Distinct();


            foreach (var userId in recipients)
            {
                await _hub.Clients
                    .User(userId.ToString())
                    .SendAsync(
                        "ReceiveTaskUpdate",
                        payload
                    );
            }
        }



        public async Task PublishCommentAddedAsync(
            TaskItem task,
            Remarks comment)
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


            // send to users currently viewing this task
            await _hub.Clients
                .Group($"Task-{task.Id}")
                .SendAsync(
                    "ReceiveComment",
                    commentPayload
                );



            // update task list for assigned/created users
            var updatePayload = new
            {
                type = "comment-added",
                taskId = task.Id
            };


            var recipients = new[]
            {
                task.AssignedToId,
                task.CreatedById
            }
            .Distinct();


            foreach (var userId in recipients)
            {
                await _hub.Clients
                    .User(userId.ToString())
                    .SendAsync(
                        "ReceiveTaskUpdate",
                        updatePayload
                    );
            }

        }
    }
}