using Microsoft.AspNetCore.Mvc;
using Task_Manager_Care.Data;

namespace Task_Manager_Care.Controllers
{
    public class ClientCategoriesController : Controller
    {
        private readonly AppDbContext _context;
        public ClientCategoriesController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
