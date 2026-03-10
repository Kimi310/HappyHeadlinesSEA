using DataAccess.Models;

namespace DataAccess;

using Microsoft.EntityFrameworkCore;

public class ProfanityDbContext : DbContext
{
    public ProfanityDbContext(DbContextOptions<ArticleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profanity> Profanities { get; set; }
}