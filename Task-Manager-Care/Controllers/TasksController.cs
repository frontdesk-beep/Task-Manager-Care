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
using Task_Manager_Care.Services;

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
        private readonly NotificationService _notificationService;
        private readonly IEmailService _emailService;

        //_context - talks to sql server
        //_hub - talks to SignalR - live work
        public TasksController(AppDbContext context, 
            IHubContext<TaskHub> hub,
            NotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _hub = hub;
            _notificationService = notificationService;
            _emailService = emailService;
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
                .Include(t => t.ServiceCategory)
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
            task.Created_On = DateTimeHelper.ToEastern(DateTime.UtcNow);
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
                    CreatedOn = task.Created_On,
                    CreatedById = task.CreatedById
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                task.ClientId = client.ClientId;
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            //use the same time stamp for the activity
            _context.TaskHistories.Add(new Activity
            {
                TaskId = task.Id,
                ChangedById = task.CreatedById,
                Action = "Created",
                ChangedAt = task.Created_On,
                Description = $"Task '{task.ClientName}' created."
            });
            await _context.SaveChangesAsync();   // Saves the history record

            // create notification for assigned user
            if (task.AssignedToId != 0)
            {
                await _notificationService.NotifyUser(
                    task.AssignedToId,
                    "Task Assigned",
                    $"Task '{task.ClientName}' assigned to you.",
                    task.Id,
                    "TaskAssigned"
                    );

                // for email sent to user for urgent tasks alert
                var assignedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == task.AssignedToId);
                Console.WriteLine($"Created By: {task.CreatedById}");
                Console.WriteLine($"Assigned To: {task.AssignedToId}");
                Console.WriteLine($"Priority: {task.PriorityId}");
                if ( task.PriorityId == 4 && assignedUser != null) // Assuming 4 is the ID for "Urgent" priority
                {
                    await _emailService.SendEmailAsync(
                        assignedUser.Email,
                        "Urgent Task Assigned",
                        $@"
                        <h2>Urgent Task Assigned</h2>
                        <p> Hello {assignedUser.Name},</p>
                        <p>A new urgent task has been assigned to you:</p>
                        
                        <p>
                        <b>Client: </b> {task.ClientName}<br/>
                        <b>Description: </b> {task.task_Description}<br/>
                        <b>Due Date: </b> {task.DueDate:d}<br/>
                        </p>
                        
                        <p>Please review it as soon as possible.</p>"
                    );
                }
                await _hub.Clients.All.SendAsync("TaskCreated", new
                {
                    task.Id,
                    task.ClientName,
                    task.AssignedToId,
                    task.StatusId,
                    task.PriorityId,
                    task.DueDate
                });
            }
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
            existing.AssignedToId = updated.AssignedToId;
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
            
            await _context.SaveChangesAsync();


            // broadcast to relevant users
            // notify old assignee if task was reassigned
            await _hub.Clients.All.SendAsync("TaskUpdated", new
            {
                id = existing.Id,
                clientName = existing.ClientName,
                assignedToId = existing.AssignedToId,
                statusId = existing.StatusId,
                priorityId = existing.PriorityId,
                dueDate = existing.DueDate
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

            await _notificationService.NotifyUser(
                    dto.AssignedToId,
                    "Task Assigned",
                    $"Task '{task.ClientName}' has been assigned to you.",
                    task.Id,
                    "TaskAssigned"
                );
            await _notificationService.NotifyUser(
                    task.CreatedById,
                    "Task Reassigned",
                    $"Task '{task.ClientName}' was reassigned from {oldUser?.Name} to {newUser?.Name}.",
                    task.Id,
                    "TaskReassigned"
                );
            await _hub.Clients.User(oldUser.Id.ToString())
                .SendAsync("TaskReassigned", new
                {
                    task.Id,
                    task.ClientName
                });

            await _context.SaveChangesAsync();
            

            await _hub.Clients.User(dto.AssignedToId.ToString())
                .SendAsync("TaskAssigned", new
                {
                    task.Id,
                    task.ClientName
                });
            await _hub.Clients.All.SendAsync("TaskUpdated", new
            {
                id = task.Id,
                clientName = task.ClientName,
                assignedToId = task.AssignedToId,
                statusId = task.StatusId,
                priorityId = task.PriorityId,
                dueDate = task.DueDate
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

            var createdById = t.CreatedById;
            var assignedToId = t.AssignedToId;
            var clientName = t.ClientName;
            var taskId = t.Id;

            _context.Tasks.Remove(t);

            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TaskDeleted", new
            {
                taskId = taskId
            });
            if (createdById == assignedToId)
            {
                await _hub.Clients.User(t.CreatedById.ToString())
                .SendAsync("TaskDeleted", new
                {
                    taskId,
                    message = $"Task '{clientName}' was deleted."
                });
            }
            else
            {
                await _hub.Clients.User(createdById.ToString())
                    .SendAsync("TaskDeleted", new
                    {
                        taskId,
                        message = $"Task '{clientName}' was deleted."
                    });
                await _hub.Clients.User(assignedToId.ToString())
                    .SendAsync("TaskDeleted", new
                    {
                        taskId,
                        message = $"Task '{clientName}' was deleted."
                    });
               
            }
            return NoContent();
        }
    }
}
    
