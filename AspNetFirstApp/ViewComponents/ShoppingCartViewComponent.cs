﻿using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SessionInfo.SessionCart) != null)
                {
                    return View(HttpContext.Session.GetInt32(SessionInfo.SessionCart) ?? 0);
                }
                else
                {
                    var shoppingCarts = await _unitOfWork.ShoppingCarts
                        .GetAllAsync(u => u.ApplicationUserId == claim.Value);
                    HttpContext.Session.SetInt32(SessionInfo.SessionCart, shoppingCarts.ToList().Count);
                    return View(HttpContext.Session.GetInt32(SessionInfo.SessionCart) ?? 0);
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
