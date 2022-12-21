using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

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
            IEnumerable<Product> products = await _unitOfWork.Products.GetAllAsync(includeProperties: "Category,CoverType");
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

        [HttpGet]
        public async ValueTask<IActionResult> Details(int productId)
        {
            var product = await _unitOfWork.Products
                .GetFirstOrDefaultAsync(p => p.Id == productId, "Category,CoverType");

            if (product == null)
            {
                NotFound();
            }

            ShoppingCart shoppingCart = new()
            {
                Product = product!,
                ProductId = productId,
                Count = 1,
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async ValueTask<IActionResult> Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                NotFound();
            }

            var cartFromDb = await _unitOfWork.ShoppingCarts
                .GetFirstOrDefaultAsync(sc=> sc.ApplicationUserId == userId && sc.ProductId == shoppingCart.ProductId);
            if (cartFromDb == null)
            {
                shoppingCart.ApplicationUserId = userId;
                await _unitOfWork.ShoppingCarts.AddAsync(shoppingCart);
                await _unitOfWork.SaveAsync();
                HttpContext.Session.SetInt32(SessionInfo.SessionCart, _unitOfWork.ShoppingCarts
                    .GetAllAsync(u => u.ApplicationUserId == userId).GetAwaiter().GetResult().ToList().Count);
            }
            else
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCarts.Update(cartFromDb);
                await _unitOfWork.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}