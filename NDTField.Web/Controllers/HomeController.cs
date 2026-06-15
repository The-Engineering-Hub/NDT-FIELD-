using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NDTField.Web.Models;

namespace NDTField.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var buildings = await _db.Buildings.ToListAsync();

        ViewBag.Total = buildings.Count;
        ViewBag.Safe = buildings.Count(b => b.SafetyStatus == "Safe");
        ViewBag.Fair = buildings.Count(b => b.SafetyStatus == "Fair");
        ViewBag.Dangerous = buildings.Count(b => b.SafetyStatus == "Dangerous");
        ViewBag.Recent = buildings
            .OrderByDescending(b => b.UploadedAt)
            .Take(3)
            .ToList();

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}