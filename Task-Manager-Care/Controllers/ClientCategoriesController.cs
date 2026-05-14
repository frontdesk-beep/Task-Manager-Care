using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Microsoft.EntityFrameworkCore;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ClientCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        //GET ALL CLIENT CATEGORIES
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientCategory>>> GetClientCategories()
        {
            var clientCategories = await _context.ClientCategories.ToListAsync();
            return Ok(clientCategories);
        }
    }
}
