using Microsoft.AspNetCore.Mvc;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Microsoft.EntityFrameworkCore;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly AppDbContext _context;
        //dependency injection
        public ClientController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Clients
                .Where(c => !c.IsDeleted)
                .ToListAsync();// Exclude deleted clients
            return Ok(clients);
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
        [HttpGet("existing")]
        public async Task<IActionResult> GetExistingClients()
        {
            var clients = await _context.Clients
                .Where(c => c.ClientCategoryId == 2)   // Existing Client
                .ToListAsync();

            return Ok(clients);
        }
        [HttpPost]
        public async Task<IActionResult> CreateClient(Client client)
        {
            client.CreatedOn = DateTime.Now;

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
