using CommentService.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CommentService.DataAccess;
public class CommentDbContextFactory : ICommentDbContextFactory
{
    private readonly DatabaseOptions _dbOptions;

    public CommentDbContextFactory(IOptions<DatabaseOptions> options)
    {
        _dbOptions = options.Value;
    }

    public CommentDbContext Create(bool isGlobal)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommentDbContext>();

        string connectionString = isGlobal
            ? _dbOptions.CommentGlobal
            : throw new Exception("Invalid server");

        optionsBuilder.UseSqlServer(connectionString);

        return new CommentDbContext(optionsBuilder.Options);
    }
}