using SubscriberService.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace SubscriberService.DataAccess;
public class SubscriberDbContextFactory : ISubscriberDbContextFactory
{
    private readonly DatabaseOptions _dbOptions;

    public SubscriberDbContextFactory(IOptions<DatabaseOptions> options)
    {
        _dbOptions = options.Value;
    }

    public SubscriberDbContext Create(bool isGlobal)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SubscriberDbContext>();

        string connectionString = isGlobal
            ? _dbOptions.SubscriberGlobal
            : throw new Exception("Invalid server");

        optionsBuilder.UseSqlServer(connectionString);

        return new SubscriberDbContext(optionsBuilder.Options);
    }
}