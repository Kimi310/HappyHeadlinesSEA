using Microsoft.AspNetCore.Mvc.ModelBinding;
using SubscriberService.DataAccess.Interfaces;
using SubscriberService.DataAccess.Models;
using SubscriberService.Service.Interfaces;

namespace SubscriberService.Service;

public class SubscriberService : ISubscriberService
{
    private readonly ISubscriberRepository _serviceRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SubscriberService> _logger;
    private readonly string _subscriberCacheUrl;

    public SubscriberService(
        ISubscriberRepository serviceRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<SubscriberService> logger
    )
    {
        _serviceRepository = serviceRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _subscriberCacheUrl = Environment.GetEnvironmentVariable("SubscriberService:Url");
    }
    
    public async Task<Subscriber> CreateSubscriberAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        var subscriber = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = email,
            Continent = "global",
            SubscribedAtUtc = DateTime.UtcNow
        };

        await _serviceRepository.CreateSubscriber(subscriber);
        
        var httpClient = _httpClientFactory.CreateClient();
        await httpClient.PostAsJsonAsync($"{_subscriberCacheUrl}/api/subscribers/cache", subscriber);
        
        return subscriber;
    }

    public async Task RemoveSubscriberAsync(Guid subscriberId)
    {
        if (subscriberId == Guid.Empty)
        {
            throw new ArgumentException("subcriberId cannot be empty" + subscriberId);
        }
        
        await _serviceRepository.RemoveSubscriber(subscriberId);
        var httpClient = _httpClientFactory.CreateClient();
        await httpClient.PostAsJsonAsync($"{_subscriberCacheUrl}/api/subscribers/cache/remove",
            new { Id = subscriberId });
        
        return ;
    }
}