using Microsoft.AspNetCore.Mvc;
using AspNetFirstApp.Data;
using AspNetFirstApp.Models;

namespace AspNetFirstApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categories = _db.Categories;
            return View(categories);
        }
    }
}
