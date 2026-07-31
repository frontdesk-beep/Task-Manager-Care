using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.Data;
using Task_Manager_Care.Helpers;
using Task_Manager_Care.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")] // reports are admin-only — see security note below
public class TaskReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    public TaskReportsController(AppDbContext context) { _context = context; }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedTasks(
        [FromQuery] int? createdById,
        [FromQuery] string? clientName,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string sortKey = "completedOn",
        [FromQuery] string sortDir = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Tasks
            .Include(t => t.Status)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.CompletedOn != null); // only completed tasks have this set

        if (createdById.HasValue)
            query = query.Where(t => t.CreatedById == createdById.Value);

        if (!string.IsNullOrWhiteSpace(clientName))
            query = query.Where(t => t.ClientName.ToLower().Contains(clientName.ToLower()));

        // Year/month filter — applied only if provided
        if (year.HasValue)
            query = query.Where(t => t.CompletedOn!.Value.Year == year.Value);

        if (month.HasValue)
            query = query.Where(t => t.CompletedOn!.Value.Month == month.Value);

        query = (sortKey, sortDir) switch
        {
            ("clientName", "asc") => query.OrderBy(t => t.ClientName),
            ("clientName", "desc") => query.OrderByDescending(t => t.ClientName),
            ("completedOn", "asc") => query.OrderBy(t => t.CompletedOn),
            _ => query.OrderByDescending(t => t.CompletedOn),
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.ClientName,
                StatusName = t.Status.Name,
                CreatedByName = t.CreatedBy.Name,
                AssignedToName = t.AssignedTo.Name,
                CompletedOn = DateTimeHelper.ToEastern(t.CompletedOn!.Value)
            })
            .ToListAsync();

        return Ok(new { totalCount, page, pageSize, items });
    }

    // Lets the frontend populate a "Year" dropdown with only years that actually have data
    [HttpGet("completed/years")]
    public async Task<IActionResult> GetAvailableYears()
    {
        var years = await _context.Tasks
            .Where(t => t.CompletedOn != null)
            .Select(t => t.CompletedOn!.Value.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        return Ok(years);
    }
}