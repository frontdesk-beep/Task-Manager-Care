using Microsoft.AspNetCore.Mvc;
using Task_Manager_Care.Data;

namespace Task_Manager_Care.Controllers
{
    public class StatusController : Controller
    {
        private readonly AppDbContext _context;
        public StatusController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
