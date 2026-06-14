using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TaskHistoryController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] int taskId)
        {
            var list = await _context.TaskHistories
                .Where(h => h.TaskId == taskId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
            return Ok(list);
        }
    }
}