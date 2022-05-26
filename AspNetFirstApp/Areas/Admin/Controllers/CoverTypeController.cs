using System.Collections;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoverTypeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.CoverTypes.ToListAsync());
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _db.CoverTypes.Add(coverType);
                await _db.SaveChangesAsync();
                TempData["success"] = "CoverType created successfully";
                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        //GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var coverType = await _db.CoverTypes.FindAsync(id);
            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _db.CoverTypes.Update(coverType);
                await _db.SaveChangesAsync();
                TempData["success"] = "CoverType edited successfully";
                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        //GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null or <= 0)
            {
                return NotFound();
            }

            var coverType = await _db.CoverTypes.FindAsync(id);
            if (coverType == null)
            {
                return NotFound();
            }

            return View(coverType);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var coverType = await _db.CoverTypes.FindAsync(id);
            if (coverType == null)
            {
                return NotFound();
            }

            _db.CoverTypes.Remove(coverType);
            await _db.SaveChangesAsync();
            TempData["success"] = "CoverType deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
