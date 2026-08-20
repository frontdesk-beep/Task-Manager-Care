using Microsoft.AspNetCore.Mvc;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Microsoft.EntityFrameworkCore;
using Task_Manager_Care.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly AppDbContext _context;
        //dependency injection
        public ClientController(AppDbContext context)
        {
            _context = context;
        }
        // GET ALL CLIENTS WITH FILTERING, SORTING, AND PAGINATION
        //use of clientquerydto to filter, sort, and paginate the clients
        [HttpGet]
        public async Task<IActionResult> GetClients(
            [FromQuery] ClientQueryDto query)
        {
            var clients = _context.Clients
                .Where(c => !c.IsDeleted)
                .AsQueryable();// Exclude deleted clients

            //Search by client name
            if(!string.IsNullOrWhiteSpace(query.Search))
            {
                clients = clients.Where(c =>
                    c.ClientName.Contains(query.Search));
            }
            //Filter by category
            if(query.CategoryId.HasValue)
            {
                clients = clients.Where(c =>
                    c.ClientCategoryId == query.CategoryId.Value);
            }
            //Filter by created date
            if(query.CreatedDate.HasValue)
            {
                var date = query.CreatedDate.Value.Date;

                clients = clients.Where(c =>
                    c.CreatedOn.Date == date.Date);
            }
            switch (query.SortBy?.ToLower())
            {
                case "clientname":
                    clients = query.SortOrder == "desc"
                        ? clients.OrderByDescending(c => c.ClientName)
                        : clients.OrderBy(c => c.ClientName);
                    break;

                case "companyname":
                    clients = query.SortOrder == "desc"
                        ? clients.OrderByDescending(c => c.CompanyName)
                        : clients.OrderBy(c => c.CompanyName);
                    break;

                case "createdon":
                    clients = query.SortOrder == "desc"
                        ? clients.OrderByDescending(c => c.CreatedOn)
                        : clients.OrderBy(c => c.CreatedOn);
                    break;

                default:
                    clients = clients.OrderBy(c => c.ClientId);
                    break;
            }
            var totalRecords = await clients.CountAsync();
            clients = clients
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);
            var result = await clients.ToListAsync();
            return Ok(new
            {
                TotalRecords = totalRecords,
                Page = query.Page,
                PageSize= query.PageSize,
                Data= result
            });

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.ClientId == id &&
                    !x.IsDeleted);

            if (client == null)
                return NotFound();

            return Ok(client);
        }
        //[HttpGet("existing")]
        //public async Task<IActionResult> GetExistingClients()
        //{
        //    var clients = await _context.Clients
        //        .Where(c => c.ClientCategoryId == 2)   // Existing Client
        //        .ToListAsync();

        //    return Ok(clients);
        //}
        [HttpPost]
        public async Task<IActionResult> CreateClient(CreateClientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

        var client = new Client
    {
        ClientName = dto.ClientName,
        ClientCategoryId = dto.ClientCategoryId,
        CompanyName = dto.CompanyName,
        PhoneNumber = dto.PhoneNumber,
        Email = dto.Email,
        Address = dto.Address,
        CreatedById= int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
        CreatedOn = DateTime.UtcNow
    };
            _context.Clients.Add(client);

            await _context.SaveChangesAsync();

            return Ok(client);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(
    int id,
    Client updated)
        {
            var client =
                await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            client.ClientCategoryId=updated.ClientCategoryId;
            client.ClientName = updated.ClientName;
            client.CompanyName = updated.CompanyName;
            client.PhoneNumber = updated.PhoneNumber;
            client.Email = updated.Email;
            client.Address = updated.Address;
            client.ClientCategoryId =
                updated.ClientCategoryId;

            await _context.SaveChangesAsync();

            return Ok(client);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(
            int id)
        {
            var client =
                await _context.Clients.FindAsync(id);

            if (client == null)
                return NotFound();

            client.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
