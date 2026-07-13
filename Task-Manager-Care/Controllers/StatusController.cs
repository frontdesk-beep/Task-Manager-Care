using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly AppDbContext _context;
        public StatusController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult GetStatuses()
        {
            var statuses = new[]
            {
                new { Id = 1, Name = "Assigned"},
                new { Id = 2, Name = "Pending" },
                new { Id = 3, Name = "In Progress" },
                new { Id = 4, Name = "Completed" },
                new { Id = 5, Name = "Cancelled" }
            };
            return Ok(statuses);
        }
    }
}