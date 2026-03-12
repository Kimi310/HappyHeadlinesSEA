namespace ProfanityService.DataAccess.Models;

public class ProfanityValidationResult
{
    public bool IsValid { get; set; }
    public IEnumerable<string> DetectedWords { get; set; } = Array.Empty<string>();
    public string? FilteredText { get; set; }
    public string? Message { get; set; }
}