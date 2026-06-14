using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL USERS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        //Add user
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        //Update user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
 int id,
 User user
)
        {
            var existing =
                await _context.Users
                .FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name =
                user.Name;

            existing.Email =
                user.Email;

            existing.Role =
                user.Role;

            // update password only if entered
            if (!string.IsNullOrEmpty(user.Password))
            {
                existing.Password =
                    user.Password;
            }

            await _context.SaveChangesAsync();

            return Ok(existing);
        }
        //Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Check if user has any incomplete tasks (created or assigned)
            var incompleteTasks = await _context.Tasks
                .Where(t => (t.CreatedById == id || t.AssignedToId == id) && t.StatusId != 3) // 3 = "Completed"
                .ToListAsync();

            if (incompleteTasks.Count > 0)
            {
                return BadRequest(new
                {
                    message = $"Cannot delete user. They have {incompleteTasks.Count} incomplete task(s). All tasks must be completed before deletion.",
                    taskCount = incompleteTasks.Count,
                    tasks = incompleteTasks.Select(t => new { t.Id, t.ClientName, t.StatusId })
                });
            }

            // All tasks completed, safe to delete
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
