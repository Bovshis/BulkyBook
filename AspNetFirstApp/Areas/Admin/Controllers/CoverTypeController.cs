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
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _unitOfWork.CoverTypes.GetAllAsync());
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.CoverTypes.AddAsync(coverType);
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

            var coverType = await _unitOfWork.CoverTypes.GetFirstOrDefaultAsync(u => u.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Edit(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverTypes.Update(coverType);
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

            var coverType = await _unitOfWork.CoverTypes.GetFirstOrDefaultAsync(u => u.Id == id);
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
            var coverType = await _unitOfWork.CoverTypes.GetFirstOrDefaultAsync(u => u.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }

            _unitOfWork.CoverTypes.Remove(coverType);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "CoverType deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
