using Microsoft.AspNetCore.SignalR;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Hubs
{
    public class TaskHub : Hub
    {
        public override Task OnConnectedAsync()
        { return base.OnConnectedAsync(); }

        public Task SubscribeToTask(int taskId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"Task-{taskId}");
        }

        public Task SubscribeToUser(int userId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
        }
    }
}
