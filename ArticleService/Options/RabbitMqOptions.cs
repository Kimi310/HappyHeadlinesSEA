namespace ArticleService.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "article_user";
    public string Password { get; set; } = "article_password";
    public string Exchange { get; set; } = "article.events";
    public string Queue { get; set; } = "article.published";
    public string RoutingKey { get; set; } = "article.published";
}

