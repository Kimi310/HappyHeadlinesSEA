namespace CommentCache.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "article.events";
    public string Queue { get; set; } = "commentcache.article.queue";
    public string RoutingKey { get; set; } = "article.published";
}