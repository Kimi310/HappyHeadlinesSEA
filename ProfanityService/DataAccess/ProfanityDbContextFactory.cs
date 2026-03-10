using ProfanityService.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace ProfanityService.DataAccess;
public class ProfanityDbContextFactory : IProfanityDbContextFactory
{
    private readonly DatabaseOptions _dbOptions;

    public ProfanityDbContextFactory(IOptions<DatabaseOptions> options)
    {
        _dbOptions = options.Value;
    }

    public ProfanityDbContext Create(bool isGlobal)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProfanityDbContext>();

        string connectionString = isGlobal 
            ? _dbOptions.ProfanityGlobal 
            : throw new Exception("Invalid server");
        optionsBuilder.UseSqlServer(connectionString);

        return new ProfanityDbContext(optionsBuilder.Options);
    }
}