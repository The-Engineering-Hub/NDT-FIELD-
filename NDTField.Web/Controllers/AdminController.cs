using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NDTField.Web.Models;
using System.Security.Claims;

namespace NDTField.Web.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AdminController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // ── GET /Admin/Login ──
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // ── POST /Admin/Login ──
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string username,
        string password,
        string? returnUrl = null)
    {
        var validUsername = _config["AdminCredentials:Username"];
        var validPassword = _config["AdminCredentials:Password"];

        if (username == validUsername &&
            password == validPassword)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,  username),
                new(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(
                claims, "AdminCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "AdminCookieAuth", principal);

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard");
        }

        ViewBag.Error = "Invalid username or password.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // ── GET /Admin/Logout ──
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AdminCookieAuth");
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Admin/Dashboard ──
    [Authorize(AuthenticationSchemes = "AdminCookieAuth")]
    public async Task<IActionResult> Dashboard(
        string? q, int page = 1)
    {
        const int pageSize = 10;

        var query = _db.Buildings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(b =>
                b.Name.Contains(q) ||
                (b.Location != null &&
                 b.Location.Contains(q)) ||
                (b.BuildingNumber != null &&
                 b.BuildingNumber.Contains(q)));

        var total = await query.CountAsync();
        var buildings = await query
            .OrderByDescending(b => b.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.TotalPages =
            (int)Math.Ceiling((double)total / pageSize);
        ViewBag.Total = total;
        ViewBag.TotalSafe =
            await _db.Buildings
                .CountAsync(b => b.SafetyStatus == "Safe");
        ViewBag.TotalFair =
            await _db.Buildings
                .CountAsync(b => b.SafetyStatus == "Fair");
        ViewBag.TotalDangerous =
            await _db.Buildings
                .CountAsync(b => b.SafetyStatus == "Dangerous");

        return View(buildings);
    }

    // ── POST /Admin/Delete/5 ──
    [Authorize(AuthenticationSchemes = "AdminCookieAuth")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id,
        string? returnPage)
    {
        var building = await _db.Buildings.FindAsync(id);
        if (building != null)
        {
            _db.Buildings.Remove(building);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Dashboard",
            new { page = returnPage ?? "1" });
    }
}