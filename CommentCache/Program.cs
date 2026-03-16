using CommentCache.Configuration;
using CommentCache.Messaging;
using CommentCache.Options;
using CommentCache.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CommentCacheConfiguration>(
    builder.Configuration.GetSection(CommentCacheConfiguration.SectionName));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
    return new CommentCacheConfiguration
    {
        RedisConnectionString = redisConnectionString,
        MaxCachedArticles = builder.Configuration.GetValue<int>("Cache:MaxCachedArticles", 30),
        CacheTtlMinutes = builder.Configuration.GetValue<int>("Cache:TtlMinutes", 60),
        KeyPrefix = builder.Configuration.GetValue<string>("Cache:KeyPrefix") ?? "comments:article:",
        LruTrackingKey = builder.Configuration.GetValue<string>("Cache:LruTrackingKey") ?? "comments:lru:tracking"
    };
});

builder.Services.AddSingleton(sp =>
{
    return new RabbitMqOptions
    {
        Host = builder.Configuration.GetValue<string>("RabbitMq:Host") ?? "localhost",
        Port = builder.Configuration.GetValue<int>("RabbitMq:Port", 5672),
        Username = builder.Configuration.GetValue<string>("RabbitMq:Username") ?? "guest",
        Password = builder.Configuration.GetValue<string>("RabbitMq:Password") ?? "guest",
        Exchange = builder.Configuration.GetValue<string>("RabbitMq:Exchange") ?? "article.events",
        Queue = builder.Configuration.GetValue<string>("RabbitMq:Queue") ?? "commentcache.article.queue",
        RoutingKey = builder.Configuration.GetValue<string>("RabbitMq:RoutingKey") ?? "article.published"
    };
});

builder.Services.AddSingleton<CommentCacheService>();
builder.Services.AddSingleton<ArticleTracker>();

builder.Services.AddHostedService<RabbitMqArticleConsumerHostedService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "CommentCache"
}));

app.MapGet("/stats/cache", async (CommentCacheService cache) =>
{
    var stats = await cache.GetStatsAsync();
    return Results.Ok(stats);
});

app.MapGet("/stats/tracking", async (ArticleTracker tracker) =>
{
    var stats = await tracker.GetStatsAsync();
    return Results.Ok(stats);
});

app.MapGet("/tracked-articles", async (ArticleTracker tracker) =>
{
    var articleIds = await tracker.GetTrackedArticleIdsAsync();
    return Results.Ok(articleIds);
});

app.MapGet("/tracked-comments", async (CommentCacheService cache, Guid articleId) =>
{
    var comments = await cache.TryGetCommentsAsync(articleId);
    return Results.Ok(comments);
});

app.Run();
