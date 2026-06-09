using System.IO.Compression;
using ArticleService.DataAccess;
using ArticleService.DataAccess.Interfaces;
using ArticleService.DataAccess.Repositories;
using ArticleService.Messaging;
using ArticleService.Options;
using ArticleService.Service.Clients;
using ArticleService.Service.Interfaces;
using DataAccess;
using Microsoft.AspNetCore.ResponseCompression;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

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
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<ArticleCacheApiOptions>(
    builder.Configuration.GetSection(ArticleCacheApiOptions.SectionName));

builder.Services.AddScoped<IArticleDbContextFactory, ArticleDbContextFactory>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService.Service.Services.ArticleService>();
builder.Services.AddHttpClient<IArticleCacheClient, ArticleCacheClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ArticleCacheApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHostedService<RabbitMqArticleConsumerHostedService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "ArticleService",
            serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSource("ArticleService.Messaging")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://tempo:4317");
            });
    });

builder.Services.AddControllers();
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

app.Run();