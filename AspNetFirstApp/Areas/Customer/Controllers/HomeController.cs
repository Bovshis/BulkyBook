using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products =
                await _unitOfWork.Products.GetAllAsync(includeProperties: "Category,CoverType");
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async ValueTask<IActionResult> Details(int id)
        {
            Product? product = await _unitOfWork.Products
                .GetFirstOrDefaultAsync(p => p.Id == id, "Category,CoverType");

            if (product == null)
            {
                NotFound();
            }

            ShoppingCart shoppingCart = new()
            {
                Product = product!,
                Count = 1,
            };

            return View(shoppingCart);
        }
    }
}