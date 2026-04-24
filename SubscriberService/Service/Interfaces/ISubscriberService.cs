using SubscriberService.DataAccess.Models;

namespace SubscriberService.Service.Interfaces;

public interface ISubscriberService
{
    public Task<Subscriber> CreateSubscriberAsync(string email);
    
    public Task RemoveSubscriberAsync(Guid subscriberId);
}