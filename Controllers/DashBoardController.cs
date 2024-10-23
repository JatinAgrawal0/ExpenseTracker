using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Syncfusion.EJ2.Charts;

namespace ExpenseTrackerApp.Controllers
{
    [Authorize] // Ensure only logged-in users can access the actions
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private DateTime _startDate;
        private DateTime _endDate;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // First day of the current month
            _endDate = DateTime.Today;
        }
        public async Task<ActionResult> FilterDates(DateTime? date_start, DateTime? date_end)
        {
            if (date_start.HasValue && date_end.HasValue)
            {
                if (date_start.Value > date_end.Value)
                {
                    ModelState.AddModelError("", "Start date cannot be greater than end date.");
                    return View("Index");
                }
                else if (!date_start.HasValue || !date_end.HasValue)
                {
                    ModelState.AddModelError("", "Both start date and end date are required.");
                    return View("Index");
                }

                else if (date_start.Value > DateTime.Now || date_end.Value > DateTime.Now)
                {
                    ModelState.AddModelError("", "Dates cannot be in the future.");
                    return View("Index");
                }
                else
                {
                    _startDate = date_start.Value;
                    _endDate = date_end.Value;

                    ViewBag.StartDate = _startDate;
                    ViewBag.EndDate = _endDate;

                    TempData["StartDate"] = _startDate;
                    TempData["EndDate"] = _endDate;
                }
            }
            else
            {
                ViewBag.StartDate = _startDate;
                ViewBag.EndDate = _endDate;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Problem("User ID could not be found.");
            }

            ViewBag.StartDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : _startDate;
            ViewBag.EndDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : _endDate;

            DateTime StartDate = ViewBag.StartDate;
            DateTime EndDate = ViewBag.EndDate;

            // Fetch transactions for the current user within the date range
            List<Transaction> selectedTransactions = await _context.JatinAgrawalTransactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate && y.UserId == userId)
                .ToListAsync();

           




            // Calculate total income
            int totalIncome = selectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = totalIncome.ToString("C0");

            // Calculate total expense
            int totalExpense = selectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = totalExpense.ToString("C0");

            // Calculate balance
            int balance = totalIncome - totalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-IN");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = balance.ToString("C0", culture);

            // Calculate total transactions
            int totalTransaction = await _context.JatinAgrawalTransactions
                .Where(y => y.Date >= StartDate && y.Date <= EndDate && y.UserId == userId)
                .CountAsync();
            ViewBag.TotalTransaction = totalTransaction;

            // Prepare data for Doughnut Chart - Expense By Category


            ViewBag.DoughnutChartData = selectedTransactions
             .Where(i => i.Category.Type == "Expense")
             .GroupBy(j => j.Category.CategoryId)
             .Select(k => new
             {
                 categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                 amount = k.Sum(j => j.Amount),
                 formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
             })
             .OrderByDescending(l => l.amount)
             .ToList();


            // Prepare data for Spline Chart - Income vs Expense
            List<SplineChartData> IncomeSummary = selectedTransactions
                 .Where(i => i.Category.Type == "Income")
                 .GroupBy(j => j.Date)
                 .Select(k => new SplineChartData()
                 {
                     day = k.First().Date.ToString("dd-MMM"),
                     income = k.Sum(l => l.Amount)
                 })
                 .ToList();


            //Expense
            List<SplineChartData> ExpenseSummary = selectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();
            ViewBag.expenseSummary = ExpenseSummary;

            //Combine Income & Expense
            int numberOfDays = (EndDate - StartDate).Days + 1;

            List<string> dateList = new List<string>();
            DateTime currentDate = StartDate; // Assuming StartDate is defined somewhere

            for (int i = 0; i < numberOfDays; i++)
            {
                dateList.Add(currentDate.ToString("dd-MMM"));
                currentDate = currentDate.AddDays(1);
            }

            string[] dateArray = dateList.ToArray();

            ViewBag.SplineChartData = from day in dateArray
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };

            // Recent Transactions
            ViewBag.RecentTransactions = await _context.JatinAgrawalTransactions
                .Include(i => i.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }

   

    public class SplineChartData
    {
        public string day { get; set; }
        public int income { get; set; }
        public int expense { get; set; }
    }
}
