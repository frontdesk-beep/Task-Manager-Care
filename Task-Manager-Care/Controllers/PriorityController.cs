using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;


namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PriorityController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PriorityController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult GetPriorities()
        {
            var priorities = new[]
            {
                new { Id = 1, Name = "Low" },
                new { Id = 2, Name = "Medium" },
                new { Id = 3, Name = "High" },
            };
            return Ok(priorities);
        }
    }
}
