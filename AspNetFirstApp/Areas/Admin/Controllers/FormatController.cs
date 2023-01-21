using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Admin)]
    public class FormatController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public FormatController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.Formats.GetAllAsync());
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Create(Format coverType)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Formats.AddAsync(coverType);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "CoverType created successfully";
                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        //GET
        public async ValueTask<IActionResult> Edit(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var coverType = await _unitOfWork.Formats.GetFirstOrDefaultAsync(u => u.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Edit(Format coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Formats.Update(coverType);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "CoverType edited successfully";
                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        //GET
        public async ValueTask<IActionResult> Delete(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var coverType = await _unitOfWork.Formats.GetFirstOrDefaultAsync(u => u.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> DeletePost(int? id)
        {
            var coverType = await _unitOfWork.Formats.GetFirstOrDefaultAsync(u => u.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }

            _unitOfWork.Formats.Remove(coverType);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "CoverType deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
