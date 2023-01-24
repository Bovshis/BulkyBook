using System.Security.Claims;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.ViewComponents
{
    public class SearchBoxViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchBoxViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync(includeProperties: "Book");
            return View(products);
        }
    }
}
