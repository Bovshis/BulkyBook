using System.Diagnostics;
using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; private set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                NotFound();
            }

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = await _unitOfWork.ShoppingCarts.GetAllAsync(u => u.ApplicationUserId == userId,
                    includeProperties: "Product")
            }; 
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.CartTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> Summary()
        {
            return View();
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetFirstOrDefaultAsync(u => u.Id == cartId);
            if (cart == null)
            {
                NotFound();
            }

            cart!.Count++;
            _unitOfWork.ShoppingCarts.Update(cart);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetFirstOrDefaultAsync(u => u.Id == cartId);
            if (cart == null)
            {
                NotFound();
            }
            if (cart!.Count <= 1)
            {
                _unitOfWork.ShoppingCarts.Remove(cart);
            }
            else
            {
                cart!.Count--;
                _unitOfWork.ShoppingCarts.Update(cart);
            }
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetFirstOrDefaultAsync(u => u.Id == cartId);
            if (cart == null)
            {
                NotFound();
            }
            _unitOfWork.ShoppingCarts.Remove(cart!);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        private static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            return quantity switch
            {
                <= 50 => price,
                <= 100 => price50,
                _ => price100
            };
        }
    }
}
