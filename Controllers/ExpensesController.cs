using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IStringLocalizer<ExpensesController> _localizer;


        public ExpensesController(AppDbContext context, IStringLocalizer<ExpensesController> localizer, UserManager<ApplicationUser> userManager)
        {
            _localizer = localizer;
            _context = context;
            _userManager = userManager;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .ToListAsync();

            return View(expenses);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            var expense = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.Categories = _context.Categories.Where(c => c.UserId == userId).ToList();
            return View();
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Date,Description,CategoryId")] Expense expense)
        {
            if (!ModelState.IsValid)
            {
                var currentUserId = _userManager.GetUserId(User);
                ViewBag.Categories = _context.Categories.Where(c => c.UserId == currentUserId).ToList();
                return View(expense);
            }

            var userId = _userManager.GetUserId(User);

            // Get current user's active budget
            var budget = await _context.Budgets
                            .Where(b => b.UserId == userId
                                     && b.StartDate <= DateTime.Now
                                     && b.EndDate >= DateTime.Now)
                            .FirstOrDefaultAsync();

            if (budget != null)
            {
                // Calculate total expenses in current budget period
                var totalExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId
                             && e.Date >= budget.StartDate
                             && e.Date <= budget.EndDate)
                    .SumAsync(e => (decimal?)e.Amount) ?? 0;

                var remaining = budget.Amount - totalExpenses;

                if (expense.Amount > remaining)
                {
                    ModelState.AddModelError("", $"Cannot add expense. Remaining budget is {remaining:N0} Ar.");
                    ViewBag.Categories = _context.Categories.Where(c => c.UserId == userId).ToList();
                    return View(expense);
                }
            }

            // Save expense if within budget
            expense.UserId = userId;
            _context.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Date,Description,CategoryId")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            // Get current user's ID
            var userId = _userManager.GetUserId(User);

            var existingExpense = await _context.Expenses
                .Where(e => e.Id == id && e.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingExpense == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only allowed fields
                    existingExpense.Amount = expense.Amount;
                    existingExpense.Date = expense.Date;
                    existingExpense.Description = expense.Description;
                    existingExpense.CategoryId = expense.CategoryId;


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Repopulate categories for dropdown
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            ViewBag.Categories = categories;
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}
