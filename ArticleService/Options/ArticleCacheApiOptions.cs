namespace ArticleService.Options;

public sealed class ArticleCacheApiOptions
{
    public const string SectionName = "ArticleCacheApi";

    public string BaseUrl { get; set; } = "http://article-cache-service:80";
}

