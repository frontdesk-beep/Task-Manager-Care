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
        public ActionResult GetClientCategories()
        {
        //hardcoded array from backend
            var clientCategories = new[]
            {
                new  { Id = 1, Name = "New Client" },
                new  { Id = 2, Name = "Existing Client" },
            };
            return Ok(clientCategories);
        }
    }
}
