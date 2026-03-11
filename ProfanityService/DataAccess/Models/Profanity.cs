namespace ProfanityService.DataAccess.Models;

public class Profanity
{
    public Guid Id  { get; set; }
    public string Word { get; set; } = string.Empty;
    public string? CreatedAt  { get; set; }
}