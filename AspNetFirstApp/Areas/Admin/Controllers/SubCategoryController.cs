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
    public class SubCategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubCategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.SubCategories.GetAllAsync(includeProperties: "Category"));
        }

        //GET
        public async Task<IActionResult> Create()
        {
            var subCategoryVm = new SubCategoryVM()
            {
                SubCategory = new SubCategory(),
                CategoryList = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name"),
            };
            return View(subCategoryVm);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Create(SubCategoryVM subCategoryVm)
        {
            if (!ModelState.IsValid) return View(subCategoryVm);

            await _unitOfWork.SubCategories.AddAsync(subCategoryVm.SubCategory);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Subcategory created successfully";

            return RedirectToAction("Index");

        }

        //GET
        public async ValueTask<IActionResult> Edit(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var subCategory = await _unitOfWork.SubCategories.GetFirstOrDefaultAsync(u=> u.Id == id, includeProperties:"Category");
            if (subCategory == null)
            {
                return NotFound();
            }

            var subCategoryVm = new SubCategoryVM()
            {
                SubCategory = subCategory,
                CategoryList = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name"),
            };
            return View(subCategoryVm);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Edit(SubCategoryVM subCategoryVm)
        {
            if (!ModelState.IsValid) return View(subCategoryVm);

            _unitOfWork.SubCategories.Update(subCategoryVm.SubCategory);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Subcategory edited successfully";

            return RedirectToAction("Index");

        }

        //GET
        public async ValueTask<IActionResult> Delete(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var subCategory = await _unitOfWork.SubCategories.GetFirstOrDefaultAsync(u=> u.Id == id, includeProperties:"Category");
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> DeletePost(int? id)
        {
            var subCategory = await _unitOfWork.SubCategories.GetFirstOrDefaultAsync(u => u.Id == id, includeProperties: "Category");
            if (subCategory == null)
            {
                return NotFound();
            }

            _unitOfWork.SubCategories.Remove(subCategory);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Subcategory deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
