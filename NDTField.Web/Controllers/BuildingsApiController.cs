using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NDTField.Web.Models;
using System.Text.Json;

namespace NDTField.Web.Controllers;

[ApiController]
[Route("api/buildings")]
public class BuildingsApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public BuildingsApiController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/buildings/upload
    // Accepts raw JSON body from NDT Field desktop app
    [HttpPost("upload")]
    public async Task<IActionResult> Upload()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return BadRequest(new { error = "Empty request body" });

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var project = root.GetProperty("project");
            var summary = root.GetProperty("summary");

            // Check for duplicate building number
            var buildingNumber = project.TryGetProperty(
                "buildingNumber", out var bn)
                ? bn.GetString() : null;

            if (!string.IsNullOrEmpty(buildingNumber))
            {
                var exists = await _db.Buildings.AnyAsync(b =>
                    b.BuildingNumber == buildingNumber);
                if (exists)
                    return Conflict(new
                    {
                        error = $"Building {buildingNumber} already exists.",
                        hint = "Delete the existing record first or use a different building number."
                    });
            }

            var building = new Building
            {
                Name = project.GetProperty("name")
                    .GetString() ?? "Unknown",
                ClientName = project.TryGetProperty("clientName",
                    out var cn) ? cn.GetString() : null,
                Location = project.TryGetProperty("location",
                    out var loc) ? loc.GetString() : null,
                BuildingNumber = buildingNumber,
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
                RawJson = body,
                UploadedAt = DateTime.UtcNow
            };

            _db.Buildings.Add(building);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                buildingId = building.Id,
                name = building.Name,
                status = building.SafetyStatus,
                score = building.SafetyScore,
                message = $"Report for {building.Name} uploaded successfully."
            });
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "Invalid JSON format." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET /api/buildings/search?q=Huruma
    [HttpGet("search")]
    public async Task<IActionResult> Search(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Query parameter q is required." });

        var results = await _db.Buildings
            .Where(b =>
                b.Name.Contains(q) ||
                (b.Location != null && b.Location.Contains(q)) ||
                (b.BuildingNumber != null && b.BuildingNumber.Contains(q)))
            .OrderByDescending(b => b.UploadedAt)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Location,
                b.BuildingNumber,
                b.SafetyStatus,
                b.SafetyScore,
                b.TotalTests,
                b.DateOfTest,
                b.UploadedAt
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET /api/buildings/{id}/status
    [HttpGet("{id}/status")]
    public async Task<IActionResult> Status(int id)
    {
        var b = await _db.Buildings.FindAsync(id);
        if (b == null) return NotFound(new { error = "Building not found." });

        return Ok(new
        {
            b.Id,
            b.Name,
            b.Location,
            b.BuildingNumber,
            b.SafetyStatus,
            b.SafetyScore,
            b.Recommendation
        });
    }

    // GET /api/buildings/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var b = await _db.Buildings.FindAsync(id);
        if (b == null) return NotFound(new { error = "Building not found." });

        return Ok(new
        {
            b.Id,
            b.Name,
            b.ClientName,
            b.Location,
            b.BuildingNumber,
            b.Operator,
            b.DateOfTest,
            b.SafetyStatus,
            b.SafetyScore,
            b.TotalTests,
            b.ExcellentCount,
            b.GoodCount,
            b.MediumCount,
            b.DoubtfulCount,
            b.Recommendation,
            b.UploadedAt
        });
    }
}