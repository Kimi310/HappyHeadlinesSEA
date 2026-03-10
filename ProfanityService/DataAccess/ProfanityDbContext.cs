using ProfanityService.DataAccess.Models;

namespace ProfanityService.DataAccess;

using Microsoft.EntityFrameworkCore;

public class ProfanityDbContext : DbContext
{
    public ProfanityDbContext(DbContextOptions<ProfanityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profanity> Profanities { get; set; }
}