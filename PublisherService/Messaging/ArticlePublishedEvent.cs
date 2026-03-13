namespace PublisherService.Messaging;

public sealed class ArticlePublishedEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Continent { get; set; } = string.Empty;
    public bool IsGlobal { get; set; }
    public DateTime PublishedAtUtc { get; set; }
}

