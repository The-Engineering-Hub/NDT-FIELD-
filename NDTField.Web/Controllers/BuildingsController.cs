using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NDTField.Web.Models;
using System.Text.Json;

namespace NDTField.Web.Controllers;

public class BuildingsController : Controller
{
    private readonly AppDbContext _db;

    public BuildingsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Buildings
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Query = q;

        var buildings = string.IsNullOrWhiteSpace(q)
            ? await _db.Buildings
                .OrderByDescending(b => b.UploadedAt)
                .ToListAsync()
            : await _db.Buildings
                .Where(b =>
                    b.Name.Contains(q) ||
                    (b.Location != null && b.Location.Contains(q)) ||
                    (b.BuildingNumber != null && b.BuildingNumber.Contains(q)))
                .OrderByDescending(b => b.UploadedAt)
                .ToListAsync();

        return View(buildings);
    }

    // GET /Buildings/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var building = await _db.Buildings.FindAsync(id);
        if (building == null) return NotFound();
        return View(building);
    }

    // GET /Buildings/Upload
    public IActionResult Upload()
    {
        return View();
    }

    // POST /Buildings/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile jsonFile)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            ViewBag.Error = "Please select a JSON file.";
            return View();
        }

        if (!jsonFile.FileName.EndsWith(".json",
            StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "Only .json files are accepted.";
            return View();
        }

        try
        {
            using var stream = jsonFile.OpenReadStream();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            // Read raw json for storage
            var rawJson = JsonSerializer.Serialize(
                root, new JsonSerializerOptions
                { WriteIndented = true });

            // Parse project info
            var project = root.GetProperty("project");
            var summary = root.GetProperty("summary");

            var building = new Building
            {
                Name = project.GetProperty("name")
                    .GetString() ?? "Unknown",
                ClientName = project.TryGetProperty("clientName",
                    out var cn) ? cn.GetString() : null,
                Location = project.TryGetProperty("location",
                    out var loc) ? loc.GetString() : null,
                BuildingNumber = project.TryGetProperty("buildingNumber",
                    out var bn) ? bn.GetString() : null,
                Operator = project.TryGetProperty("operator",
                    out var op) ? op.GetString() : null,
                DateOfTest = project.TryGetProperty("dateOfTest",
                    out var dt) && DateTime.TryParse(
                        dt.GetString(), out var parsed)
                    ? parsed : null,
                SafetyStatus = summary.GetProperty("safetyStatus")
                    .GetString(),
                SafetyScore = summary.GetProperty("safetyScore")
                    .GetInt32(),
                TotalTests = summary.GetProperty("totalTests")
                    .GetInt32(),
                ExcellentCount = summary.GetProperty("excellentCount")
                    .GetInt32(),
                GoodCount = summary.GetProperty("goodCount")
                    .GetInt32(),
                MediumCount = summary.GetProperty("mediumCount")
                    .GetInt32(),
                DoubtfulCount = summary.GetProperty("doubtfulCount")
                    .GetInt32(),
                Recommendation = summary.TryGetProperty("recommendation",
                    out var rec) ? rec.GetString() : null,
                RawJson = rawJson,
                UploadedAt = DateTime.UtcNow
            };

            _db.Buildings.Add(building);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = building.Id });
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Failed to parse JSON: {ex.Message}";
            return View();
        }
    }
}