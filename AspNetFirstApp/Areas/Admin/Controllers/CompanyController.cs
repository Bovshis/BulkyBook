using System.Collections;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRole.Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async ValueTask<IActionResult> Upsert(int? id)
        {
            if (id is null or 0)
            {
                return View(new Company());
            }

            var company = await _unitOfWork.Companies.GetFirstOrDefaultAsync(filter: x => x.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async ValueTask<IActionResult> Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    await _unitOfWork.Companies.AddAsync(company);
                }
                else
                {
                    _unitOfWork.Companies.Update(company);
                }

                await _unitOfWork.SaveAsync();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }

            return View(company);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyList = await _unitOfWork.Companies.GetAllAsync();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public async ValueTask<IActionResult> Delete(int id)
        {
            var company = await _unitOfWork.Companies.GetFirstOrDefaultAsync(u => u.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Companies.Remove(company);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Company deleted successfully";
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
