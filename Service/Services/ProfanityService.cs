using DataAccess.Models;
using Services.Interfaces;

namespace Services.Services;

public class ProfanityService: IProfanityService
{
    public Task<bool> ContainsProfanityAsync(string text)
    {
        throw new NotImplementedException();
    }

    public Task<string> FilterProfanityAsync(string text, char replacementChar = '*')
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetDetectedProfanityAsync(string text)
    {
        throw new NotImplementedException();
    }

    public Task<ProfanityValidationResult> ValidateCommentAsync(string comment)
    {
        throw new NotImplementedException();
    }
}