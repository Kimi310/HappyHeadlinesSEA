using ArticleCacheService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleCacheService.DataAccess;

public class ArticleDbContext(DbContextOptions<ArticleDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }
}

