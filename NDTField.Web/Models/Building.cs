using System.ComponentModel.DataAnnotations;

namespace NDTField.Web.Models;

public class Building
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? Location { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Operator { get; set; }
    public DateTime? DateOfTest { get; set; }
    public string? SafetyStatus { get; set; }
    public int SafetyScore { get; set; }
    public int TotalTests { get; set; }
    public int ExcellentCount { get; set; }
    public int GoodCount { get; set; }
    public int MediumCount { get; set; }
    public int DoubtfulCount { get; set; }
    public string? Recommendation { get; set; }
    public string? RawJson { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}