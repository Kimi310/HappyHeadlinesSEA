namespace SubscriberService.DataAccess.Models;

public class Subscriber
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Continent { get; set; } = string.Empty;
    public DateTime SubscribedAtUtc { get; set; } = DateTime.UtcNow;
}