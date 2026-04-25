using SubscriberService.DataAccess.Interfaces;
using SubscriberService.DataAccess.Models;

namespace SubscriberService.DataAccess.Repositories;

public class SubscriberRepository : ISubscriberRepository
{
    private readonly ISubscriberDbContextFactory _factory;

    public SubscriberRepository(ISubscriberDbContextFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<Subscriber> CreateSubscriber(Subscriber comment)
    {
        var context = _factory.Create(true);
        context.Subscribers.Add(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task RemoveSubscriber(Guid subscriberId)
    {
        var context = _factory.Create(true);
        var subscriber = await context.Subscribers.FindAsync(subscriberId);
    
        if (subscriber is null)
            return;
    
        context.Subscribers.Remove(subscriber);
        await context.SaveChangesAsync();
    }
}