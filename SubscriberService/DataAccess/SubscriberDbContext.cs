using Microsoft.EntityFrameworkCore;
using SubscriberService.DataAccess.Models;

namespace SubscriberService.DataAccess;

public class SubscriberDbContext : DbContext
{
    public SubscriberDbContext(DbContextOptions<SubscriberDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subscriber> Subscribers { get; set; }
}