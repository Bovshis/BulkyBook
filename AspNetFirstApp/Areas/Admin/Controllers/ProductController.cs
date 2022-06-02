using System.Collections;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public async ValueTask<IActionResult> Upsert(int? id)
        {
            var productVM = new ProductVM
            {
                Product = new Product(),
                CategoryList = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name"),
                CoverTypeList = new SelectList(await _unitOfWork.CoverTypes.GetAllAsync(), "Id", "Name"),
            };

            if (id is null or 0)
            {
                return View(productVM);
            }

            var product = await _unitOfWork.Products.GetFirstOrDefaultAsync(filter: x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            productVM.Product = product;
            return View(productVM);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    if (productVM.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    string fileName = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));
                    string uploads = Path.Combine(wwwRootPath, @"images\products");
                    await using var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                    await file.CopyToAsync(fileStream);
                    productVM.Product.ImageUrl = Path.Combine(@"\images\products", fileName);
                }

                if (productVM.Product.Id == 0)
                {
                    await _unitOfWork.Products.AddAsync(productVM.Product);
                }
                else
                {
                    _unitOfWork.Products.Update(productVM.Product);
                }

                await _unitOfWork.SaveAsync();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }

            return View(productVM);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productList = await _unitOfWork.Products.GetAllAsync(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }

        [HttpDelete]
        public async ValueTask<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetFirstOrDefaultAsync(u => u.Id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if (product.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Product deleted successfully";
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
