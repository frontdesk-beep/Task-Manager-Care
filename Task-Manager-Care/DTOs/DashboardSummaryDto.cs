namespace Task_Manager_Care.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalClients { get; set; }
        public int TotalEmployees { get; set; }
        public int OpenTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverDueTasks { get; set; }
        public int PendingTasks { get; set; }

    }
}
