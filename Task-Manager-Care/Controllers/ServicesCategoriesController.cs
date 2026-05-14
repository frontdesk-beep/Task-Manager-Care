using Microsoft.AspNetCore.Mvc;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Microsoft.EntityFrameworkCore;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesCategoriesController : Controller
    {
        private readonly AppDbContext _context;
        public ServicesCategoriesController(AppDbContext context)
        {
            _context = context;
        }
        //GET ALL SERVICE CATEGORIES
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCategory>>> GetServiceCategories()
        {
            var serviceCategories = await _context.ServiceCategories.ToListAsync();
            return Ok(serviceCategories);
        }
    }
}
