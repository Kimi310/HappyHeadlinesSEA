using DataAccess.Models;

namespace DataAccess;

using Microsoft.EntityFrameworkCore;

public class CommentDbContext : DbContext
{
    public CommentDbContext(DbContextOptions<ArticleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comments { get; set; }
}