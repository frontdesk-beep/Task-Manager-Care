namespace Task_Manager_Care.Services
{
    using Microsoft.AspNetCore.SignalR;
    using Task_Manager_Care.Data;
    using Task_Manager_Care.DTOs;
    using Task_Manager_Care.Helpers;
    using Task_Manager_Care.Hubs;
    using Task_Manager_Care.Models;

    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<TaskHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task NotifyUser(
                int userId,
                string title,
                string message,
                int? taskId = null,
                string type = "")
        {
            var notification = new Notification
            {
                UserId = userId,
                TaskId = taskId,

                Title = title,
                Message = message,
                Type = type,

                IsRead = false,

                CreatedOn = DateTimeHelper.ToEastern(DateTime.UtcNow)
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var dto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedOn = notification.CreatedOn,
                TaskId = notification.TaskId
            };

            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", dto);
        }
    }

}
