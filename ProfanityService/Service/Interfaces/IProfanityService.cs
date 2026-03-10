using ProfanityService.DataAccess.Models;

namespace ProfanityService.Service.Interfaces;

public interface IProfanityService
{
    // Filters profanity from the provided text
    Task<string> FilterProfanityAsync(string text, char replacementChar = '*');

    Task<bool> ContainsProfanityAsync(string text);
}
