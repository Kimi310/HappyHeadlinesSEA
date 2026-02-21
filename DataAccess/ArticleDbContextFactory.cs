using DataAccess.Interfaces;



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace DataAccess;
public class ArticleDbContextFactory : IArticleDbContextFactory
{
    private readonly DatabaseOptions _dbOptions;

    public ArticleDbContextFactory(IOptions<DatabaseOptions> options)
    {
        _dbOptions = options.Value;
    }

    public ArticleDbContext Create(string continent, bool isGlobal)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();

        string connectionString = isGlobal
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