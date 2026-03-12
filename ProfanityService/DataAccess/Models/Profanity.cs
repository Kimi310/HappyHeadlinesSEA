namespace ProfanityService.DataAccess.Models;

public class Profanity
{
    public Guid Id  { get; set; }
    public string Word { get; set; } = string.Empty;
    public DateTime? CreatedAt  { get; set; }
}