using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data; 
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context; 
    private readonly IStringLocalizer<HomeController> _localizer;
     private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, AppDbContext context, IStringLocalizer<HomeController> localizer)
    {
        _context = context;
        _logger = logger;
        _localizer = localizer;
        _userManager = userManager;
    }

    public IActionResult Index()

    {
        var userId = _userManager.GetUserId(User);

         var localizedDashboardTitle = _localizer["DashboardTitle"];
        _logger.LogInformation($"Localized DashboardTitle: {localizedDashboardTitle}");
        _logger.LogInformation($"Resource path: {typeof(HomeController).FullName}");

         // Total amount spent
        var totalExpenses = _context.Expenses
            .Where(e => e.UserId == userId)
            .Sum(e => e.Amount);

        // Group by category
        var expensesByCategory = _context.Expenses
            .Where(e => e.UserId == userId)
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

    public IActionResult SetLanguage(string culture, string returnUrl = null)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl ?? "/");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
