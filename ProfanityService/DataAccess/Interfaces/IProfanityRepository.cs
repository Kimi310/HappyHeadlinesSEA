using ProfanityService.DataAccess.Models;

namespace ProfanityService.DataAccess.Interfaces;

public interface IProfanityRepository
{
    // Get a list of profanities
    public Task<List<Profanity>> GetProfaneWords();
}