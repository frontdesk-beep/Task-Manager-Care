using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//for real time notifications
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task_Manager_Care.Data;
using Task_Manager_Care.DTOs;
using Task_Manager_Care.Helpers;
using Task_Manager_Care.Hubs;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        //ControllerBase - means this class contains the all task apis 
        private readonly AppDbContext _context;
        private readonly IHubContext<TaskHub> _hub;

        //_context - talks to sql server
        //_hub - talks to SignalR - live work
        public TasksController(AppDbContext context, IHubContext<TaskHub> hub)
        {
            _context = context;
            _hub = hub;    
        }

        // GET /api/tasks?assignedToId=5&createdById=0
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTasks(
            [FromQuery] int? AssignedToId, 
            [FromQuery] int? CreatedById,
            [FromQuery] bool activeOnly = false
            )
        {
            //if we use the include then assignedtoid with name,email,role, assignedto
            //without include - only assignedtoid
            //select * from tasks - includes properties of assignedto,createdby,status,priority,service- like a join query bcoz, tasks table does not contain values which we want,
            //so it is for example, assignedto but -> users table for all employee related items.
            //AssignedToId → Users,  CreatedById → Users, StatusId → Statuses, PriorityId → Priorities, ServiceCategoryId → ServiceCategories
            var q = _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Status)
                .Include(t => t.PriorityNavigation)
                .Include(t=> t.ServiceCategory)
                .AsQueryable();
            //Asqueryable means i am still building my query
            //if assignedtoid has value then add assignedtoid = value same for createdbyid
            if (AssignedToId.HasValue)
                q = q.Where(t => t.AssignedToId == AssignedToId.Value);

            if (CreatedById.HasValue)
                q = q.Where(t => t.CreatedById == CreatedById.Value);
            if (activeOnly)
            {
                q = q.Where(t =>
                    t.Status.Name != "Completed" &&
                    t.Status.Name != "Cancelled");
            }

            //now sql runs
            var tasks = await q.ToListAsync();

            //instead of sending whole entity we are sending only required jason
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
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Status)
                .Include(t => t.PriorityNavigation)
                .Include(t => t.ServiceCategory)
                .Where(t =>
                    t.Status.Name != "Completed" &&
                    t.Status.Name != "Cancelled")
                .ToListAsync();

            return Ok(tasks.Select(t => new
            {
                t.Id,
                t.ClientName,
                t.ClientCategoryId,
                t.PhoneNumber,
                t.Email,
                task_Description = t.task_Description,

                t.AssignedToId,
                assignedToName = t.AssignedTo.Name,

                t.CreatedById,
                createdByName = t.CreatedBy.Name,

                t.StatusId,
                statusName = t.Status.Name,

                t.PriorityId,
                priorityName = t.PriorityNavigation.Name,

                t.ServiceCategoryId,
                ServiceCategoryName = t.ServiceCategory.Name,

                t.DueDate,
                t.Created_On
            }));
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
            
            if (task == null) 
                return NotFound();

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
            if (task.ClientId != null)
            {
                var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == task.ClientId);

                if (client == null)
                    return BadRequest("Client not found");

                task.ClientName = client.ClientName;
                task.PhoneNumber = client.PhoneNumber;
                task.Email = client.Email;
                task.ClientCategoryId = client.ClientCategoryId;
            }
            else
            {
                var client = new Client
                {
                    ClientName = task.ClientName,
                    ClientCategoryId = task.ClientCategoryId,
                    PhoneNumber = task.PhoneNumber,
                    Email = task.Email,
                    CreatedOn = DateTimeHelper.ToEastern(DateTime.UtcNow),
                    CreatedById = task.CreatedById
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                task.ClientId = client.ClientId;
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _context.TaskHistories.Add(new Activity
            {
                TaskId = task.Id,
                ChangedById = task.CreatedById,
                Action = "Created",
                ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                Description = $"Task '{task.ClientName}' created."
            });
            await _context.SaveChangesAsync();   // Saves the history record

            // create notification for assigned user
            if (task.AssignedToId != 0)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedToId,
                    TaskId = task.Id,
                    Message = $"Task '{task.ClientName}' assigned to you.",
                    CreatedOn = DateTimeHelper.ToEastern(DateTime.UtcNow)
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // notify via SignalR: target user by NameIdentifier (user id string)
                await _hub.Clients.User(task.AssignedToId.ToString())
                          .SendAsync("TaskAssigned", new { taskId = task.Id, message = notification.Message });
            }

            // also broadcast to creator
            await _hub.Clients.User(task.CreatedById.ToString())
                      .SendAsync("TaskCreated", new
                      {
                          task.Id,
                          task.ClientName,
                          task.StatusId,
                          task.AssignedToId
                      });

            return Ok(new
            {
                task.Id,
                task.ClientId,
                task.ClientName,
                task.PhoneNumber,
                task.Email,
                task.ClientCategoryId,
                task.AssignedToId,
                task.StatusId,
                task.PriorityId,
                task.ServiceCategoryId,
                task.DueDate,
                task.Created_On
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updated)
        {
            var existing = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existing == null) 
                return NotFound();

            //save the old assignee
            var oldAssignedToId = existing.AssignedToId;
            //Load the old assigned user for history record
            var oldAssignedUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == oldAssignedToId);

            var newAssignedUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == updated.AssignedToId);

            // only update allowed fields (safe update)
            var oldStatus = existing.StatusId;
            var oldPriority = existing.PriorityId;
            var oldDueDate = existing.DueDate;
            var oldServiceCategory = existing.ServiceCategoryId;
            existing.StatusId = updated.StatusId;
            var completedStatusId = await _context.Statuses
                .Where(s => s.Name == "Completed")
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (updated.StatusId == completedStatusId)
            {
                if (existing.CompletedOn == null)
                    existing.CompletedOn = DateTimeHelper.ToEastern(DateTime.Now);
            }
            else
            {
                existing.CompletedOn = null;
            }
            //existing.AssignedToId = updated.AssignedToId;
            existing.task_Description = updated.task_Description;
            existing.DueDate = updated.DueDate;
            existing.PriorityId = updated.PriorityId;
            existing.ServiceCategoryId = updated.ServiceCategoryId;
            existing.ClientName = updated.ClientName;
            existing.PhoneNumber = updated.PhoneNumber;
            existing.Email = updated.Email;

            await _context.SaveChangesAsync();
            var oldStatusName = await _context.Statuses
                .Where(x => x.Id == oldStatus)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            var newStatusName = await _context.Statuses
                .Where(x => x.Id == updated.StatusId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
            var oldPriorityName = await _context.Priorities
                .Where(x => x.Id == oldPriority)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            var newPriorityName = await _context.Priorities
                .Where(x => x.Id == updated.PriorityId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
            var oldService = await _context.ServiceCategories
                .Where(x => x.Id == oldServiceCategory)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            var newService = await _context.ServiceCategories
                .Where(x => x.Id == updated.ServiceCategoryId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();


            // record history
            if (oldStatus != updated.StatusId)
            {
                _context.TaskHistories.Add(new Activity
                {
                    TaskId = existing.Id,
                    ChangedById = int.Parse(User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    Action = "Status Changed",
                    ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                    Description = $"Status changed from {oldStatusName} to {newStatusName}"
                });
            }
          
            if (oldPriority != updated.PriorityId)
            {
                _context.TaskHistories.Add(new Activity
                {
                    TaskId = existing.Id,
                    ChangedById = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
                    Action = "Priority Changed",
                    ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                    Description = $"Priority changed from {oldPriorityName} to {newPriorityName}"
                });
            }
            if (oldDueDate != updated.DueDate)
            {
                _context.TaskHistories.Add(new Activity
                {
                    TaskId = existing.Id,
                    ChangedById = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
                    Action = "Due Date Changed",
                    ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                    Description = $"Due date changed from {oldDueDate:d} to {updated.DueDate:d}"
                });
            }
            if (oldServiceCategory != updated.ServiceCategoryId)
            {
                _context.TaskHistories.Add(new Activity
                {
                    TaskId = existing.Id,
                    ChangedById = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
                    Action = "Service Category Changed",
                    ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                    Description = $"Service changed from '{oldService}' to '{newService}'"
                });
            }
            // add notification for new assigned user
            var note = new Notification
            {
                UserId = existing.AssignedToId,
                TaskId = existing.Id,
                Message = $"Task '{existing.ClientName}' updated. Status: {existing.StatusId}",
                CreatedOn = DateTimeHelper.ToEastern(DateTime.UtcNow)   
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
            await _hub.Clients.User(existing.AssignedToId.ToString())
                .SendAsync("TaskUpdated", new
                {
                    existing.Id,
                    existing.ClientName,
                    existing.StatusId,
                    existing.AssignedToId,
                    existing.DueDate
                });
            return NoContent();
        }

        //reassigning the task
        [HttpPut("{id}/reassign")]
        public async Task<IActionResult> ReassignTask(int id, ReassignTaskDto dto)
        {
            var task = await _context.Tasks
                .Include(x => x.AssignedTo)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return NotFound();
            if (task.AssignedToId == dto.AssignedToId)
            {
                return BadRequest("Task is already assigned to this user.");
            }

            var oldUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == task.AssignedToId);

            var newUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == dto.AssignedToId);

            task.AssignedToId = dto.AssignedToId;

            await _context.SaveChangesAsync();

            _context.TaskHistories.Add(new Activity
            {
                TaskId = task.Id,
                ChangedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                Action = "Reassigned",
                ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                Description = $"Task reassigned from {oldUser?.Name} to {newUser?.Name}"
            });

            if (!string.IsNullOrWhiteSpace(dto.Comment))
            {
                _context.Comments.Add(new Remarks
                {
                    TaskId = task.Id,
                    UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                    Text = dto.Comment,
                    CreatedAt = DateTimeHelper.ToEastern(DateTime.UtcNow)
                });
            }

            _context.Notifications.Add(new Notification
            {
                UserId = dto.AssignedToId,
                TaskId = task.Id,
                Message = $"Task '{task.ClientName}' has been assigned to you.",
                CreatedOn = DateTimeHelper.ToEastern(DateTime.UtcNow)
            });

            await _context.SaveChangesAsync();

            await _hub.Clients.User(dto.AssignedToId.ToString())
                .SendAsync("TaskAssigned", new
                {
                    task.Id,
                    task.ClientName
                });

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {

            var t = await _context.Tasks.FindAsync(id);

            if (t == null)
                return NotFound();

            _context.TaskHistories.Add(new Activity
            {
                TaskId = t.Id,
                ChangedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                Action = "Deleted",
                ChangedAt = DateTimeHelper.ToEastern(DateTime.UtcNow),
                Description = $"Task '{t.ClientName}' deleted."
            });

            _context.Tasks.Remove(t);
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TaskDeleted", new { taskId = id });
            return NoContent();
        }
    }
}