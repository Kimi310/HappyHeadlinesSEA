using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class ProfanityRepository : IProfanityRepository
{
    private readonly IProfanityDbContextFactory _factory;

    public ProfanityRepository(IProfanityDbContextFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<List<Profanity>> GetProfaneWords()
    {
        var context = _factory.Create(true);
        var response = await context.Profanities.ToListAsync();
        return response;
    }
}