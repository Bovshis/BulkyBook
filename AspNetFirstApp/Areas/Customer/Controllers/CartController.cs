using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                NotFound();
            }

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCarts.Set
                    .Include(s => s.Product)
                        .ThenInclude(p => p.Book)
                    .Where(s => s.ApplicationUserId == userId),
                OrderHeader = new OrderHeader(),
            }; 
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = cart.Product.SalePrice;
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> Summary()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                NotFound();
            }

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCarts.Set
                    .Include(s => s.Product)
                    .ThenInclude(p => p.Book)
                    .Where(s => s.ApplicationUserId == userId),
                OrderHeader = new OrderHeader(),
            };

            var user = await _unitOfWork.ApplicationUsers.GetFirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                NotFound();
            }

            ShoppingCartVM.OrderHeader.ApplicationUser = user!;
            ShoppingCartVM.OrderHeader.Name = user!.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = user!.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = user!.StreetAddress;
            ShoppingCartVM.OrderHeader.City = user!.City;
            ShoppingCartVM.OrderHeader.State = user!.State;
            ShoppingCartVM.OrderHeader.PostalCode = user!.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = cart.Product.SalePrice;
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                NotFound();
            }

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCarts.Set
                .Include(s => s.Product)
                    .ThenInclude(p => p.Book)
                .Where(s => s.ApplicationUserId == userId);
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId!;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = cart.Product.SalePrice;
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            ApplicationUser? applicationUser = await _unitOfWork.ApplicationUsers
                .GetFirstOrDefaultAsync(u => u.Id == userId);
            if (applicationUser == null)
            {
                NotFound();
            }

            if (applicationUser!.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.Pending;
                ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.Pending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = PaymentStatus.DelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.Approved;
            }


            await _unitOfWork.OrderHeaders.AddAsync(ShoppingCartVM.OrderHeader);
            await _unitOfWork.SaveAsync();
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            }
            await _unitOfWork.SaveAsync();

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:44356/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"Customer/Cart/Index",
                };

                foreach (var item in ShoppingCartVM.ListCart)
                {

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Book.Title
                            },

                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);

                }

                var service = new SessionService();
                Session session = await service.CreateAsync(options);
                ShoppingCartVM.OrderHeader.SessionId = session.Id;
                ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;
                await _unitOfWork.SaveAsync();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader? orderHeader = await _unitOfWork.OrderHeaders.GetFirstOrDefaultAsync(u => u.Id == id);
            if (orderHeader == null)
            {
                BadRequest();
            }
            if (orderHeader!.PaymentStatus != PaymentStatus.DelayedPayment)
            {
                var service = new SessionService();
                Session session = await service.GetAsync(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    orderHeader.OrderStatus = OrderStatus.Approved;
                    orderHeader.PaymentStatus = PaymentStatus.Approved;
                }
            }
            var shoppingCarts = await _unitOfWork.ShoppingCarts
                .GetAllAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId);
            _unitOfWork.ShoppingCarts.RemoveRange(shoppingCarts);
            await _unitOfWork.SaveAsync();
            return View(id);
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
                await _unitOfWork.SaveAsync();
                HttpContext.Session.SetInt32(SessionInfo.SessionCart, _unitOfWork.ShoppingCarts
                    .GetAllAsync(u => u.ApplicationUserId == cart.ApplicationUserId).GetAwaiter().GetResult().ToList().Count);
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
            HttpContext.Session.SetInt32(SessionInfo.SessionCart, _unitOfWork.ShoppingCarts
                .GetAllAsync(u => u.ApplicationUserId == cart.ApplicationUserId).GetAwaiter().GetResult().ToList().Count);

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
