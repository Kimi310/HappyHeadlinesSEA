namespace ArticleCacheService.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = string.Empty;

    public string ToConfigurationString()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            return $"{Host}:{Port}";
        }

        return $"{Host}:{Port},password={Password}";
    }
}

