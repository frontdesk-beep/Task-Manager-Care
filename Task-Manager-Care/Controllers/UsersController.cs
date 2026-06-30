using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Task_Manager_Care.DTOs;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly PasswordHasher<User> _hasher =
            new PasswordHasher<User>();

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL ACTIVE USERS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .ToListAsync();

            return Ok(users);
        }

        // CREATE USER
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto dto)
        {
            // Prevent duplicate email
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "Email already exists."
                });
            }

            // Map CreateUserDto to User model
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = _hasher.HashPassword(null, dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                DOB = dto.DOB
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // UPDATE USER
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUserDto dto
        )
        {
            var existing =
                await _context.Users.FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.Role = dto.Role;

            // Password NOT updated here
            // Use Profile/Forgot Password instead

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // SOFT DELETE USER
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user =
                await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Check incomplete tasks
            var incompleteTasks = await _context.Tasks
                .Where(t =>
                    (t.CreatedById == id ||
                     t.AssignedToId == id)
                    &&
                    t.StatusId != 3
                )
                .ToListAsync();

            if (incompleteTasks.Any())
            {
                return BadRequest(new
                {
                    message =
                        $"Cannot deactivate user. They have {incompleteTasks.Count} incomplete task(s).",
                    taskCount =
                        incompleteTasks.Count
                });
            }

            // Soft Delete
            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Employee deactivated successfully."
            });
       
    } 
    
        [HttpPut("reactivate/{id}")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsActive = true;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Employee reactivated successfully."
            });
        }
    }}