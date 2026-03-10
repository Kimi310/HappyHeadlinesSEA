using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IProfanityRepository
{
    // Get a list of profanities
    public Task<List<Profanity>> GetProfaneWords();
}