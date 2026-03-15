using ArticleCacheService.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArticleCacheService.DataAccess;

public class ArticleDbContextFactory(IOptions<DatabaseOptions> options) : IArticleDbContextFactory
{
    private readonly DatabaseOptions _dbOptions = options.Value;

    public ArticleDbContext Create(string continent, bool isGlobal)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();

        var connectionString = isGlobal
            ? _dbOptions.Global
            : GetConnectionString(continent);

        optionsBuilder.UseSqlServer(connectionString);
        return new ArticleDbContext(optionsBuilder.Options);
    }

    private string GetConnectionString(string continent)
    {
        return continent switch
        {
            "Europe" => _dbOptions.Europe,
            "Asia" => _dbOptions.Asia,
            "Africa" => _dbOptions.Africa,
            "NorthAmerica" => _dbOptions.NorthAmerica,
            "SouthAmerica" => _dbOptions.SouthAmerica,
            "Australia" => _dbOptions.Australia,
            "Antarctica" => _dbOptions.Antarctica,
            _ => throw new Exception("Invalid continent")
        };
    }
}

