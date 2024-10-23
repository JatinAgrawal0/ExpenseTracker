
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerApp.Data;
using ExpenseTrackerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTrackerApp.Controllers
{
    [Authorize] // Ensure only logged-in users can access the actions
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TransactionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            var transactions = await _context.JatinAgrawalTransactions
                                             .Where(t => t.UserId == userId)
                                             .Include(t => t.Category)
                                             .ToListAsync();

            return View(transactions);
        }

        // GET: Transaction/AddOrEdit
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            PopulateCategories(userId);

            if (id == 0)
            {
                return View(new Transaction { UserId = userId });
            }
            else
            {
                var transaction = await _context.JatinAgrawalTransactions
                                                .Where(t => t.TransactionId == id && t.UserId == userId)
                                                .Include(t => t.Category)
                                                .FirstOrDefaultAsync();
                if (transaction == null)
                {
                    return NotFound();
                }
                return View(transaction);
            }
        }

        // POST: Transaction/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            transaction.UserId = userId; // Ensure the transaction is associated with the current user

            // Manual validation
            bool isValid = true;
            if (transaction.CategoryId <= 0)
            {
                ModelState.AddModelError(nameof(transaction.CategoryId), "Category is required.");
                isValid = false;
            }
            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError(nameof(transaction.Amount), "Amount must be greater than zero.");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(transaction.Note))
            {
                ModelState.AddModelError(nameof(transaction.Note), "Note is required.");
                isValid = false;
            }
            if (transaction.Date == default)
            {
                ModelState.AddModelError(nameof(transaction.Date), "Date is required.");
                isValid = false;
            }

            if (!isValid)
            {
                PopulateCategories(userId);
                return View(transaction);
            }

            if (transaction.TransactionId == 0)
            {
                _context.Add(transaction);
            }
            else
            {
                var existingTransaction = await _context.JatinAgrawalTransactions
                                                        .Where(t => t.TransactionId == transaction.TransactionId && t.UserId == userId)
                                                        .FirstOrDefaultAsync();
                if (existingTransaction == null)
                {
                    return NotFound();
                }

                // Update properties
                existingTransaction.CategoryId = transaction.CategoryId;
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.Note = transaction.Note;
                existingTransaction.Date = transaction.Date;

                _context.Update(existingTransaction);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            var transaction = await _context.JatinAgrawalTransactions
                                            .Where(t => t.TransactionId == id && t.UserId == userId)
                                            .FirstOrDefaultAsync();
            if (transaction == null)
            {
                return NotFound();
            }

            _context.JatinAgrawalTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public void PopulateCategories(string userId)
        {
            var categories = _context.JatinAgrawalCategories
                                     .Where(c => c.UserId == userId)
                                     .ToList();
            categories.Insert(0, new Category { CategoryId = 0, Title = "Choose a Category" });
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Title");
        }
    }
}
