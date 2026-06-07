namespace ArticleCacheService.Options;

public sealed class ArticleCacheOptions
{
    public const string SectionName = "ArticleCache";

    public bool Enabled { get; set; } = true;
    public int L1ExpirationMinutes { get; set; } = 5;
    public int L2ExpirationMinutes { get; set; } = 30;
    public int PreloadIntervalMinutes { get; set; } = 15;
    public int PreloadDays { get; set; } = 14;
}

