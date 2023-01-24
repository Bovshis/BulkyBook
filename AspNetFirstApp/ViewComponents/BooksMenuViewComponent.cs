using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.ViewComponents
{
    public class BooksMenuViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public BooksMenuViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(includeProperties: "SubCategories");
            return View(categories);
        }
    }
}
