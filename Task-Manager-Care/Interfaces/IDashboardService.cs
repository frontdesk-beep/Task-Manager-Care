using Task_Manager_Care.DTOs;

namespace Task_Manager_Care.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummary();

        Task<DashboardSummaryDto> GetMySummary(int userId);
        Task<List<RecentTaskDto>> GetRecentTasks();
        Task<List<NotificationDto>> GetNotifications(int userId);
    }
}
