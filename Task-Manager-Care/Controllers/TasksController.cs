using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

            public TasksController(AppDbContext context)
            {
                _context = context;
            }

            // GET ALL TASKS
            [HttpGet]
            public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
            {
                var tasks = await _context.Tasks.ToListAsync();

                return Ok(tasks);
            }

            // GET TASK BY ID
            [HttpGet("{id}")]
            public async Task<ActionResult<TaskItem>> GetTask(int id)
            {
                var task = await _context.Tasks.FindAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

                return Ok(task);
            }

            // CREATE TASK
            [HttpPost]
            public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
            {
                _context.Tasks.Add(task);

                await _context.SaveChangesAsync();

                return Ok(task);
            }

            // UPDATE TASK
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateTask(int id, TaskItem task)
            {
                if (id != task.Id)
                {
                    return BadRequest();
                }

                _context.Entry(task).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }

            // DELETE TASK
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteTask(int id)
            {
                var task = await _context.Tasks.FindAsync(id);

                if (task == null)
                {
                    return NotFound();
                }

                _context.Tasks.Remove(task);

                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    }

