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
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
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
            var productVm = new ProductVM()
            {
                Product = new Product(),
                FormatList = new SelectList(await _unitOfWork.Formats.GetAllAsync(), "Id", "Name"),
                BookList = new SelectList(await _unitOfWork.Books.GetAllAsync(), "Id", "DataTextFieldLabel"),
            };

            if (id is null or 0)
            {
                return View(productVm);
            }

            var product = await _unitOfWork.Products.GetFirstOrDefaultAsync(filter: x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            productVm.Product = product;
            return View(productVm);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVm, IFormFile? file)
        {
            if (!ModelState.IsValid) return View(productVm);

            if (productVm.Product.Id == 0)
            {
                await _unitOfWork.Products.AddAsync(productVm.Product);
            }
            else
            {
                _unitOfWork.Products.Update(productVm.Product);
            }

            await _unitOfWork.SaveAsync();
            TempData["success"] = "Product created successfully";
            return RedirectToAction(nameof(Index));

        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _unitOfWork.Products.GetAllAsync(includeProperties:"Format");
            return Json(new { data = products });
        }

        [HttpDelete]
        public async ValueTask<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetFirstOrDefaultAsync(u => u.Id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Product deleted successfully";
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
