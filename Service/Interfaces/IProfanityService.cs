using DataAccess.Models;

namespace Services.Interfaces;

public interface IProfanityService
{

    // Filters profanity from the provided text
    Task<bool> ContainsProfanityAsync(string text);
    
    // Gets a list of profanity detected
    Task<string> FilterProfanityAsync(string text, char replacementChar = '*');
    
    // Gets a list of profane words detected in the text.
    Task<IEnumerable<string>> GetDetectedProfanityAsync(string text);
    
    // Validates if a comment is acceptable (does not contain profanity).
    Task<ProfanityValidationResult> ValidateCommentAsync(string comment);
}
