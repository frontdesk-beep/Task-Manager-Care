using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Manager_Care.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Task_Manager_Care.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    { 
        private readonly IDashboardService _dashboardService;
        
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _dashboardService.GetSummary();
            return Ok(result);
        }

        [HttpGet("my-summary")]
        public async Task<IActionResult> GetMySummary()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var result = await _dashboardService.GetMySummary(userId);

            return Ok(result);
        }

        [HttpGet("recent-tasks")]
        public async Task<IActionResult> GetRecentTasks()
        {
            var tasks = await _dashboardService.GetRecentTasks();
            return Ok(tasks);
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            int userId = int.Parse(userIdClaim.Value);
            var result = await _dashboardService.GetNotifications(userId);
            return Ok(result);
        }
    }
}
