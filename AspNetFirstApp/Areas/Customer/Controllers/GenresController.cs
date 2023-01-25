using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class GenresController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public GenresController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Category(int categoryId)
        {
            var books = _unitOfWork.Books.Set
                .Include(b => b.Products)
                .Include(b => b.SubCategory)
                .ThenInclude(s => s.Category)
                .Where(b => b.SubCategory.CategoryId == categoryId)
                .AsEnumerable();
            var grouping = books.GroupBy(b => b.SubCategory.Category.Name);
            var dict = grouping.ToDictionary(g => g.Key, g => g.ToList());
            return View(dict);
        }

        public async Task<IActionResult> Subcategory(int subcategoryId)
        {
            var books =  await _unitOfWork.Books
                .GetAllAsync(b => b.SubCategoryId == subcategoryId, includeProperties: "Products,SubCategory");
            return View(books);
        }

        public async Task<IActionResult> GetBooks(int subcategoryId, IEnumerable<string>? authors = null)
        {
            var books = await _unitOfWork.Books
                .GetAllAsync(b => b.SubCategoryId == subcategoryId, includeProperties: "Products,SubCategory");
            if (authors != null)
            {
                books = books.Where(b => authors.Contains(b.Author));
            }

            return PartialView("_GetBooks", books);
        }
    }
}
