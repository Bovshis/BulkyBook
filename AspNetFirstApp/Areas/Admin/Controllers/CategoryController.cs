using System.Collections;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.Categories.GetAllAsync());
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        //GET
        public async ValueTask<IActionResult> Edit(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(u=> u.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Category edited successfully";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        //GET
        public async ValueTask<IActionResult> Delete(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(u=> u.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> DeletePost(int? id)
        {
            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
