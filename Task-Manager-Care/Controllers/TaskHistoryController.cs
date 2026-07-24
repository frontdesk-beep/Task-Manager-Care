using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Helpers;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskHistoryController : ControllerBase
    {
        //ACTIVITY TAB
        private readonly AppDbContext _context;
        public TaskHistoryController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetHistory([FromQuery] int taskId)
        {
            var list = await _context.TaskHistories
                .Include(x => x.ChangedBy)
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Action,
                    x.Description,
                    ChangedAt = DateTimeHelper.ToEastern(x.ChangedAt),
                    ChangedBy = x.ChangedBy.Name
        })
        .ToListAsync();

     return Ok(list);
        }
    }
}