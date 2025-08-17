using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ExpenseTracker.Data; 

namespace ExpenseTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context; 

    public HomeController(ILogger<HomeController> logger,AppDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
         // Total amount spent
        var totalExpenses = _context.Expenses.Sum(e => e.Amount);

        // Group by category
        var expensesByCategory = _context.Expenses
            .Include(e => e.Category)
            .GroupBy(e => e.Category.Name)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .ToList();

        ViewData["TotalExpenses"] = totalExpenses;
        ViewData["ExpensesByCategory"] = expensesByCategory;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
