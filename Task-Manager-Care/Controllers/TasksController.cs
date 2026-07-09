using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//for real time notifications
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Task_Manager_Care.Hubs;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskHub> _hub;

        public TasksController(AppDbContext context, IHubContext<TaskHub> hub)
        {
            _context = context;
            _hub = hub;
            
        }

        // GET /api/tasks?assignedToId=5&createdById=0
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTasks([FromQuery] int? AssignedToId, [FromQuery] int? CreatedById)
        {
            var q = _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Status)
                .Include(t => t.PriorityNavigation)
                .Include(t=> t.ServiceCategory)
                .AsQueryable();

            if (AssignedToId.HasValue)
                q = q.Where(t => t.AssignedToId == AssignedToId.Value);

            if (CreatedById.HasValue)
                q = q.Where(t => t.CreatedById == CreatedById.Value);

            var tasks = await q.ToListAsync();

            var result = tasks.Select(t => new
            {
                t.Id,
                t.ClientName,
                task_Description = t.task_Description,
                t.AssignedToId,
                t.ServiceCategoryId,
                ServiceCategoryName = t.ServiceCategory?.Name,
                t.ClientCategoryId,
                assignedToName = t.AssignedTo?.Name,
                t.CreatedById,
                createdByName = t.CreatedBy?.Name,
                t.StatusId,
                statusName = t.Status?.Name,
                t.PriorityId,
                priorityName = t.PriorityNavigation?.Name,
                t.DueDate,
                t.Created_On,
                t.PhoneNumber,
                t.Email
            });

            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Status)
                .Include(t => t.PriorityNavigation)
                .Include(t => t.ServiceCategory)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            var result = new
            {
                task.Id,
                task.ClientName,
                task.ClientCategoryId,
                task.ServiceCategoryId,
                ServiceCategoryName = task.ServiceCategory?.Name,
                task.task_Description,
                task.AssignedToId,
                assignedToName = task.AssignedTo?.Name,
                task.CreatedById,
                createdByName = task.CreatedBy?.Name,
                task.StatusId,
                statusName = task.Status?.Name,
                task.PriorityId,
                priorityName = task.PriorityNavigation?.Name,
                task.DueDate,
                task.Created_On,
                task.PhoneNumber,
                task.Email,
            };
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            task.Created_On = task.Created_On == default ? DateTime.UtcNow : task.Created_On;
            // CHECK IF CLIENT EXISTS
            var existingClient =
                await _context.Clients
                .FirstOrDefaultAsync(c =>
                    (c.Email == task.Email || 
                    c.PhoneNumber ==  task.PhoneNumber) 
                && !c.IsDeleted);

            // CREATE CLIENT IF NOT EXISTS
            if (existingClient == null)
            {
                var client = new Client
                {
                    ClientName = task.ClientName,
                    ClientCategoryId = task.ClientCategoryId,
                    PhoneNumber = task.PhoneNumber,
                    Email = task.Email,
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = task.CreatedById
                };
                _context.Clients.Add(client);
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // create notification for assigned user
            if (task.AssignedToId != 0)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedToId,
                    TaskId = task.Id,
                    Message = $"Task '{task.ClientName}' assigned to you.",
                    CreatedOn = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // notify via SignalR: target user by NameIdentifier (user id string)
                await _hub.Clients.User(task.AssignedToId.ToString())
                          .SendAsync("TaskAssigned", new { taskId = task.Id, message = notification.Message });
            }

            // also broadcast to creator
            await _hub.Clients.User(task.CreatedById.ToString())
                      .SendAsync("TaskCreated", task);

            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updated)
        {
            var existing = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existing == null) return NotFound();
            // only update allowed fields (safe update)
            var oldStatus = existing.StatusId;
            existing.StatusId = updated.StatusId;
            existing.AssignedToId = updated.AssignedToId;
            existing.task_Description = updated.task_Description;
            existing.DueDate = updated.DueDate;
            existing.PriorityId = updated.PriorityId;
            existing.ServiceCategoryId = updated.ServiceCategoryId;
            existing.ClientName = updated.ClientName;
            existing.PhoneNumber = updated.PhoneNumber;
            existing.Email = updated.Email;

            await _context.SaveChangesAsync();

            // record history
            _context.TaskHistories.Add(new TaskHistory
            {
                TaskId = existing.Id,
                ChangedById = int.Parse(User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"),
                OldStatusId = oldStatus,
                NewStatusId = existing.StatusId,
                ChangedAt = DateTime.UtcNow,
                Note = $"Status changed from {oldStatus} to {existing.StatusId}"
            });

            // add notification for assigned user
            var note = new Notification
            {
                UserId = existing.AssignedToId,
                TaskId = existing.Id,
                Message = $"Task '{existing.ClientName}' updated. Status: {existing.StatusId}",
                CreatedOn = DateTime.UtcNow
            };
            _context.Notifications.Add(note);

            await _context.SaveChangesAsync();
            // Broadcast with full details
            var taskDetail = new
            {
                existing.Id,
                existing.ClientName,
                task_Description = existing.task_Description,
                existing.AssignedToId,
                assignedToName = existing.AssignedTo?.Name,
                existing.CreatedById,
                createdByName = existing.CreatedBy?.Name,
                existing.StatusId,
                statusName = existing.Status?.Name,
                existing.DueDate
            };


            // broadcast to relevant users
            await _hub.Clients.User(existing.AssignedToId.ToString()).SendAsync("TaskUpdated", existing);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var t = await _context.Tasks.FindAsync(id);
            if (t == null) return NotFound();
            _context.Tasks.Remove(t);
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TaskDeleted", new { taskId = id });
            return NoContent();
        }
    }
}