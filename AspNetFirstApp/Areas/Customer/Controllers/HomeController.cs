using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
            IEnumerable<Book> books = await _unitOfWork.Books.GetAllAsync(includeProperties: "SubCategory,Products");
            return View(books);
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
        public async Task<IActionResult> Details(int productId)
        {
            var product = await _unitOfWork.Products.Set
                .Include(p=> p.Book)
                    .ThenInclude(b => b.SubCategory)
                        .ThenInclude(s => s.Category)
                .Include(p => p.Book)
                    .ThenInclude(b => b.Products)
                        .ThenInclude(p => p.Format)    
                .Include(p => p.Format)
                .FirstOrDefaultAsync(b => b.Id == productId);
            
            if (product == null)
            {
                NotFound();
            }

            product!.Book.Products = product.Book.Products.OrderBy(p => p.FormatId);

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
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
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