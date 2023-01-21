using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = await _unitOfWork.OrderHeaders
                    .GetFirstOrDefaultAsync(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = await _unitOfWork.OrderDetails
                    .GetAllAsync(u => u.OrderId == orderId, includeProperties: "Product"),
            };
            return View(OrderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details_PAY_NOW()
        {
            OrderVM.OrderHeader = await _unitOfWork.OrderHeaders
                .GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = await _unitOfWork.OrderDetails
                .GetAllAsync(u => u.OrderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //stripe settings 
            var domain = "https://localhost:44356/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetail)
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
            OrderVM.OrderHeader.SessionId = session.Id;
            OrderVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;
            await _unitOfWork.SaveAsync();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader? orderHeader = await _unitOfWork.OrderHeaders
                .GetFirstOrDefaultAsync(u => u.Id == orderHeaderId);
            if (orderHeader == null)
            {
                NotFound();
            }

            if (orderHeader!.PaymentStatus == PaymentStatus.DelayedPayment)
            {
                var service = new SessionService();
                Session session = await service.GetAsync(orderHeader.SessionId);
                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    orderHeader.PaymentStatus = PaymentStatus.Approved;
                    await _unitOfWork.SaveAsync();
                }
            }
            return View(orderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeaders
                .GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb == null)
            {
                NotFound();
            }
            
            orderHeaderFromDb!.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeaders.Update(orderHeaderFromDb);
            await _unitOfWork.SaveAsync();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartProcessing()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeaders
                .GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb == null)
            {
                NotFound();
            }

            orderHeaderFromDb!.OrderStatus = OrderStatus.InProcess;
            _unitOfWork.OrderHeaders.Update(orderHeaderFromDb);
            await _unitOfWork.SaveAsync();
            TempData["Success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeaders
                .GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeader == null)
            {
                NotFound();
            }
            if (orderHeader!.PaymentStatus == PaymentStatus.DelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            orderHeader!.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = OrderStatus.Shipped;
            orderHeader.ShippingDate = DateTime.Now;
            _unitOfWork.OrderHeaders.Update(orderHeader);
            await _unitOfWork.SaveAsync();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = UserRole.Admin + "," + UserRole.Employee)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeaders
                .GetFirstOrDefaultAsync(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeader == null)
            {
                NotFound();
            }

            if (orderHeader!.PaymentStatus == PaymentStatus.Approved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = await service.CreateAsync(options);
                orderHeader.OrderStatus = OrderStatus.Cancelled;
                orderHeader.PaymentStatus = PaymentStatus.Refunded;
            }
            else
            {
                orderHeader.OrderStatus = OrderStatus.Cancelled;
                orderHeader.PaymentStatus = PaymentStatus.Cancelled;
            }

            _unitOfWork.OrderHeaders.Update(orderHeader);
            await _unitOfWork.SaveAsync();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(UserRole.Admin) || User.IsInRole(UserRole.Employee))
            {
                orderHeaders = await _unitOfWork.OrderHeaders.GetAllAsync(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    NotFound();
                }
                orderHeaders = await _unitOfWork.OrderHeaders
                    .GetAllAsync(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            orderHeaders = status switch
            {
                "pending" => orderHeaders.Where(u => u.PaymentStatus == PaymentStatus.DelayedPayment),
                "inprocess" => orderHeaders.Where(u => u.OrderStatus == OrderStatus.InProcess),
                "completed" => orderHeaders.Where(u => u.OrderStatus == OrderStatus.Shipped),
                "approved" => orderHeaders.Where(u => u.OrderStatus == OrderStatus.Approved),
                _ => orderHeaders
            };
            return Json(new { data = orderHeaders });
        }

        #endregion

    }
}
