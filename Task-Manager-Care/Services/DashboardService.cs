using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Task_Manager_Care.Data;
using Task_Manager_Care.DTOs;
using Task_Manager_Care.Interfaces;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Services
{
    public class DashboardService(AppDbContext context) : IDashboardService
    {
        private readonly AppDbContext _context=context;

        //creates an empty object
        public async Task<DashboardSummaryDto> GetSummary()
        {
            DashboardSummaryDto summary= new DashboardSummaryDto();
            //throw new NotImplementedException();

            //Total Clients
            //select COUNT(*) from Clients
            summary.TotalClients = await _context.Clients.CountAsync();

            //Total Employees
            //select COUNT(*) from Users
            summary.TotalEmployees= await _context.Users.CountAsync();

            //Open Tasks
            //SELECT COUNT(*) FROM Tasks WHERE StatusId = 3 - except complted all other 3 tasks are open
            summary.OpenTasks= await _context.Tasks.CountAsync(t => t.Status.Name != "Completed");

            //Completed Tasks
            summary.CompletedTasks= await _context.Tasks.CountAsync(t => t.Status.Name == "Completed");

            //Pending Tasks
            summary.PendingTasks=await _context.Tasks.CountAsync(t => t.Status.Name == "Pending");
            //OverDue Tasks
            summary.OverDueTasks= await _context.Tasks
                .CountAsync(t =>
                t.DueDate < DateTime.Now &&
                t.Status.Name != "Completed");
        return summary;
        }
        public async Task<DashboardSummaryDto> GetMySummary(int userId)
        {
            DashboardSummaryDto summary = new DashboardSummaryDto();
            // Total Clients assigned to the user
            summary.TotalClients = await _context.Clients
                .CountAsync(c => c.CreatedById == userId);
            summary.TotalEmployees = 0;

            summary.OpenTasks = await _context.Tasks
            .CountAsync(t =>
            t.AssignedToId == userId &&
            t.Status.Name != "Completed");

            summary.PendingTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name == "Pending");

            summary.CompletedTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name == "Completed");

            summary.OverDueTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name != "Completed" &&
                    t.DueDate < DateTime.Today);

           return summary;

        }

        public async Task<List<RecentTaskDto>> GetRecentTasks()
        {
            //similar to this query-SELECT TOP 5 t.Id,t.ClientName,u.Name,p.Name,s.NameFROM Tasks tINNER JOIN Users uON t.AssignedToId = u.IdINNER JOIN Statuses sON t.StatusId = s.IdINNER JOIN Priorities pON t.PriorityId = p.IdORDER BY t.Created_On DESC
            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Status)
                .Include(t => t.PriorityNavigation)
                .OrderByDescending(t => t.Created_On)
                .Take(5)
                .Select(t => new RecentTaskDto
                {
                    Id = t.Id,
                    ClientName = t.ClientName!,
                    AssignedTo = t.AssignedTo!.Name,
                    Priority = t.PriorityNavigation!.Name,
                    Status = t.Status!.Name,
                    CreatedOn = t.Created_On
                })
                .ToListAsync();

            return tasks;
        }

        public async Task<List<NotificationDto>> GetNotifications(int userId)
        {
            return await _context.Notifications
                                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedOn)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedOn = n.CreatedOn,
                    TaskId = n.TaskId
                })
                .ToListAsync();
        }

        public Task<TaskChartDto> GetTaskChart()
        {
            throw new NotImplementedException();
        }
    }
}
