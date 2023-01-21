using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Admin)]
    public class BookController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BookController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var bookVm = new BookVM()
            {
                Book = new Book()
                {
                    SubCategory = new SubCategory()
                },
                CategoryList = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name"),
                SubCategoryList = new SelectList(await _unitOfWork.SubCategories.GetAllAsync(), "Id", "Name"),
            };

            if (id is null or 0)
            {
                return View(bookVm);
            }

            var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(filter: x => x.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            bookVm.Book = book;
            return View(bookVm);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(BookVM bookVm, IFormFile? file)
        {
            if (!ModelState.IsValid) return View(bookVm);
            bookVm.Book.SubCategory =
                await _unitOfWork.SubCategories.GetFirstOrDefaultAsync(s => s.Id == bookVm.Book.SubCategoryId);
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                if (bookVm.Book.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, bookVm.Book.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                string fileName = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));
                string uploads = Path.Combine(wwwRootPath, @"images\products");
                await using var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                await file.CopyToAsync(fileStream);
                bookVm.Book.ImageUrl = Path.Combine(@"\images\products", fileName);
            }

            if (bookVm.Book.Id == 0)
            {
                await _unitOfWork.Books.AddAsync(bookVm.Book);
            }
            else
            {
                _unitOfWork.Books.Update(bookVm.Book);
            }

            await _unitOfWork.SaveAsync();
            TempData["success"] = "Book created successfully";
            return RedirectToAction("Index");

        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _unitOfWork.Books.GetAllAsync(includeProperties: "SubCategory");
            return Json(new { data = books });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(includeProperties: "SubCategories");
            var result = Json(categories);
            return result;
        }

        [HttpDelete]
        public async ValueTask<IActionResult> Delete(int id)
        {
            var book = await _unitOfWork.Books.GetFirstOrDefaultAsync(u => u.Id == id);
            if (book == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            if (book.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, book.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _unitOfWork.Books.Remove(book);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Product deleted successfully";
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
