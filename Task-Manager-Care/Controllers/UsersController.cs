using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Task_Manager_Care.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> GetUsers(
            [FromQuery] EmployeeQueryDto query)
        {
            var users = _context.Users
                .Where(u=>u.Role != "SuperAdmin")
                .AsQueryable();

            //search by name or email
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                users = users.Where(u =>
                    u.Name.Contains(query.Search) ||
                    u.Email.Contains(query.Search));
            }
            //filter by active status
            if(query.IsActive.HasValue)
            {
                users = users.Where(u => 
                u.IsActive == query.IsActive.Value);
            }
            //filter by role
            if(!string.IsNullOrWhiteSpace(query.Role))
            {
                users = users.Where(u => 
                u.Role == query.Role);
            }
            switch(query.SortBy?.ToLower())
            {
                case "name":
                    users = query.SortOrder == "desc"
                        ? users.OrderByDescending(u => u.Name)
                        : users.OrderBy(u => u.Name);
                    break;
                case "email":
                    users = query.SortOrder == "desc"
                        ? users.OrderByDescending(u => u.Email)
                        : users.OrderBy(u => u.Email);
                    break;
                case "role":
                    users = query.SortOrder == "desc"
                        ? users.OrderByDescending(u => u.Role)
                        : users.OrderBy(u => u.Role);
                    break;
                default:
                    users = query.SortOrder == "desc"
                        ? users.OrderByDescending(u => u.Name)
                        : users.OrderBy(u => u.Name);
                    break;
            }
            var totalRecords = await users.CountAsync();

            users = users
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var result = await users.ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                Page = query.Page,
                PageSize = query.PageSize,
                Data = result
            });
        }

        // CREATE USER
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto dto)
        {
            //find role
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
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
            if (currentRole == "Admin" && dto.Role != "Employee")
            {
                return Forbid();
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
        [Authorize(Roles = "Admin,SuperAdmin")]

        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUserDto dto
        )
        {
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var existing =
                await _context.Users.FindAsync(id);

            if (existing == null)
            {
                return NotFound();
            }
            if (existing.Role == "SuperAdmin")
            {
                return Forbid();
            }

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            if (currentRole == "Admin")
            {
                if (dto.Role != "Employee")
                    return Forbid();
            }
            existing.Role = dto.Role;
            // Password NOT updated here
            // Use Profile/Forgot Password instead

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // SOFT DELETE USER
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user =
                await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            if (user.Role == "SuperAdmin")
            {
                return Forbid();
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
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Role == "SuperAdmin")
            {
                return Forbid();
            }
            user.IsActive = true;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Employee reactivated successfully."
            });
        }
    }}