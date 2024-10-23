
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Grids;

namespace ExpenseTrackerApp.Controllers
{
    [Authorize] // Ensure only logged-in users can access the actions
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Get the current user's ID
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            var categories = await _context.JatinAgrawalCategories
                                           .Where(c => c.UserId == userId)
                                           .ToListAsync();




            return View(categories);
        }

        // GET: Category/AddOrEdit
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Problem("User could not be found.");
            }

            if (id == 0)
            {
                // Create a new category
                return View(new Category { UserId = user.Id });
            }
            else
            {
                var category = await _context.JatinAgrawalCategories
                                             .Where(c => c.CategoryId == id && c.UserId == user.Id)
                                             .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        // POST: Category/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type,UserId")] Category category)
        {


            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            category.UserId = userId; // Ensure the category is associated with the current user

            if (category.CategoryId == 0)
            {

                _context.Add(category);

            }
            else
            {
                var existingCategory = await _context.JatinAgrawalCategories
                                                     .Where(c => c.CategoryId == category.CategoryId && c.UserId == userId)
                                                     .FirstOrDefaultAsync();
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // Update properties
                existingCategory.Title = category.Title;
                existingCategory.Icon = category.Icon;
                existingCategory.Type = category.Type;

                _context.Update(existingCategory);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            var category = await _context.JatinAgrawalCategories
                                         .Where(c => c.CategoryId == id && c.UserId == userId)
                                         .FirstOrDefaultAsync();
            if (category == null)
            {
                return NotFound();
            }

            _context.JatinAgrawalCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

