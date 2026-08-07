using Microsoft.AspNetCore.SignalR;
using System;

namespace Task_Manager_Care.Hubs
{
    public class TaskHub : Hub
    {

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"SignalR Connected User: {userId}");

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"User-{userId}"
                );
            }
            await base.OnConnectedAsync();
        }

        public async Task SubscribeToTask(int taskId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"Task-{taskId}"
            );
        }
        // User leaves task details page
        public async Task LeaveTask(int taskId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"Task-{taskId}"
            );

            Console.WriteLine(
                $"User {Context.UserIdentifier} left Task-{taskId}"
            );
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

    }
}