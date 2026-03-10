using CommentService.DataAccess.Models;

namespace CommentService.DataAccess;

using Microsoft.EntityFrameworkCore;

public class CommentDbContext : DbContext
{
    public CommentDbContext(DbContextOptions<CommentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comments { get; set; }
}