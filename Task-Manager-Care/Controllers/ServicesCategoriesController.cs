using Microsoft.AspNetCore.Mvc;
using Task_Manager_Care.Data;
using Task_Manager_Care.Models;
using Microsoft.EntityFrameworkCore;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ServicesCategoriesController(AppDbContext context)
        {
            _context = context;
        }
        //GET ALL SERVICE CATEGORIES
        [HttpGet]
        public ActionResult GetServiceCategories()
        {
            var serviceCategories = new[]
            {
                new { Id = 1, Name = "Investment" },
                new { Id = 2, Name = "Insurance" },
                new { Id = 3, Name = "TAX" },
                new { Id = 4, Name = "Real Estate" },
                new { Id = 5, Name = "Mortgage" },
                new { Id = 6, Name = "Travel Insurance" },
                new { Id = 7, Name = "Financial Planning" },
                new { Id = 8, Name = "Others" },
            };
            return Ok(serviceCategories);
        }
    }
}
