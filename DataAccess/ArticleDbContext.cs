using DataAccess.Models;

namespace DataAccess;

using Microsoft.EntityFrameworkCore;

public class ArticleDbContext : DbContext
{
    public ArticleDbContext(DbContextOptions<ArticleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles { get; set; }
}