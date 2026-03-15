namespace CommentCache.Models;

/// <summary>
/// Event published when an article is created
/// Used to track the 30 most recently published articles for caching
/// </summary>
public class ArticlePublishedEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Continent { get; set; } = string.Empty;
    public bool IsGlobal { get; set; }
    public DateTime PublishedAtUtc { get; set; }
}