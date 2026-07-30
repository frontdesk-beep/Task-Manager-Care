using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Task_Manager_Care.Data;
using Task_Manager_Care.DTOs;
using Task_Manager_Care.Helpers;
using Task_Manager_Care.Interfaces;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Services
{
    public class DashboardService(AppDbContext context) : IDashboardService
    {
        private readonly AppDbContext _context=context;

        //creates an empty object
        //for admin all tasks
        public async Task<DashboardSummaryDto> GetSummary()
        {
            DashboardSummaryDto summary = new DashboardSummaryDto();
            //throw new NotImplementedException();

            //Total Clients
            //select COUNT(*) from Clients
            summary.TotalClients = await _context.Clients.CountAsync();

            //Total Employees
            //select COUNT(*) from Users
            summary.TotalEmployees = await _context.Users.CountAsync();

            //Open Tasks
            //SELECT COUNT(*) FROM Tasks WHERE StatusId = 3 - except complted all other 3 tasks are open
            summary.OpenTasks = await _context.Tasks
                .CountAsync(t => 
                t.Status.Name != "Completed" &&
                t.Status.Name != "Cancelled");

            //Completed Tasks
            summary.CompletedTasks = await _context.Tasks.CountAsync(t => t.Status.Name == "Completed");
            var today = DateTimeHelper.ToEastern(DateTime.UtcNow).Date;

            //Pending Tasks
            summary.PendingTasks = await _context.Tasks.CountAsync(t => t.Status.Name == "Pending");
            //OverDue Tasks
            summary.OverDueTasks = await _context.Tasks
                .CountAsync(t =>
                t.DueDate.Date < today &&
                t.Status.Name != "Completed" &&
                t.Status.Name != "Cancelled");

            summary.AssignedTasks = await _context.Tasks
                .CountAsync(t =>
                t.Status.Name == "Assigned"
                && t.Status.Name != "Completed");

            //urgent Tasks
            summary.UrgentTasks = await _context.Tasks
                .CountAsync(t =>
                t.PriorityNavigation.Name == "Urgent" &&
                t.Status.Name != "Completed" &&
                t.Status.Name != "Cancelled");
         
            return summary;
        }
        public async Task<DashboardSummaryDto> GetMySummary(int userId)
        {
            DashboardSummaryDto summary = new DashboardSummaryDto();
            var today = DateTimeHelper.ToEastern(DateTime.UtcNow).Date;

            // Total Clients assigned to the user
            summary.TotalClients = await _context.Clients
                .CountAsync(c => c.CreatedById == userId);
            summary.TotalEmployees = 0;

            summary.OpenTasks = await _context.Tasks
            .CountAsync(t =>
            t.AssignedToId == userId &&
            t.Status.Name != "Completed" &&
            t.Status.Name != "Cancelled");

            summary.PendingTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name == "Pending");

            summary.AssignedTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name == "Assigned" &&
                    t.Status.Name != "Completed");

            summary.CompletedTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name == "Completed");

            summary.OverDueTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.Status.Name != "Completed" &&
                     t.Status.Name != "Cancelled" &&
                    t.DueDate.Date < today);

            summary.UrgentTasks = await _context.Tasks
                .CountAsync(t =>
                    t.AssignedToId == userId &&
                    t.PriorityNavigation.Name == "Urgent" &&
                    t.Status.Name != "Completed" 
                    && t.Status.Name != "Cancelled");

            return summary;

        }

        public async Task<List<RecentTaskDto>> GetRecentTasks()
        {
            //similar to this query-SELECT TOP 10 t.Id,t.ClientName,u.Name,p.Name,s.NameFROM Tasks tINNER JOIN Users uON t.AssignedToId = u.IdINNER JOIN Statuses sON t.StatusId = s.IdINNER JOIN Priorities pON t.PriorityId = p.IdORDER BY t.Created_On DESC
            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Status)
                .Include(t => t.PriorityNavigation)
                .OrderByDescending(t => t.Created_On)
                .Take(10)
                .Select(t => new RecentTaskDto
                {
                    Id = t.Id,
                    ClientName = t.ClientName!,
                    AssignedTo = t.AssignedTo!.Name,
                    Priority = t.PriorityNavigation!.Name,
                    Status = t.Status!.Name,
                    Created_On = t.Created_On
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
