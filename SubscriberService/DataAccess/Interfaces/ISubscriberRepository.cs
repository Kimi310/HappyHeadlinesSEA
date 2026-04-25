using SubscriberService.DataAccess.Models;

namespace SubscriberService.DataAccess.Interfaces;

public interface ISubscriberRepository
{
    public Task<Subscriber> CreateSubscriber(Subscriber subscriber);
    
    public Task RemoveSubscriber(Guid subscriberId);
}