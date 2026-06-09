using ArticleCacheService.DataAccess;
using ArticleCacheService.DataAccess.Interfaces;
using ArticleCacheService.DataAccess.Repositories;
using System.IO.Compression;
using ArticleCacheService.Options;
using ArticleCacheService.Service;
using ArticleCacheService.Service.Interfaces;
using ArticleCacheService.Service.Services;
using ArticleCacheService.Service.Telemetry;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<ArticleCacheOptions>(builder.Configuration.GetSection(ArticleCacheOptions.SectionName));

var cacheOptions = builder.Configuration.GetSection(ArticleCacheOptions.SectionName).Get<ArticleCacheOptions>() ?? new ArticleCacheOptions();
builder.Services.AddSingleton(new CacheRuntimeState(cacheOptions.Enabled));

var compressionEnabled = builder.Configuration.GetValue("Compression:Enabled", true);
if (compressionEnabled)
{
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
}

var redisOptions = builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();

builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisOptions.ToConfigurationString();
    options.InstanceName = "happyheadlines:";
});
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions.ToConfigurationString()));

builder.Services.AddScoped<IArticleDbContextFactory, ArticleDbContextFactory>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleCacheService, ArticleCacheService.Service.Services.ArticleCacheService>();
builder.Services.AddSingleton<IArticleCacheMetricsStore, RedisArticleCacheMetricsStore>();
builder.Services.AddHostedService<ArticleCachePreloadHostedService>();

var app = builder.Build();

if (compressionEnabled)
{
    app.UseResponseCompression();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseStatusCodePages();
app.UseCors(config =>
    config.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());

app.MapControllers();

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();

