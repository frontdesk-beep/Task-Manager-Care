using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public NotificationsController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int userId)
        {
            var notifications = await _context.Notifications
                .Where(n=> n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

                return Ok(notifications);
        }
        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return NotFound();
            n.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new {success = true});
        }
    }
}