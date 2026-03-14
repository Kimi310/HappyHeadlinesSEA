using NewsletterService.DataAccess;
// using NewsletterService.DataAccess.Interfaces;
// using NewsletterService.DataAccess.Repositories;
using NewsletterService.Messaging;
using NewsletterService.Options;
using NewsletterService.Service.Interfaces;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configuration
// builder.Services.Configure<DatabaseOptions>(
//     builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Data Access
// builder.Services.AddScoped<ISubscriberDbContextFactory, SubscriberDbContextFactory>();
// builder.Services.AddScoped<ISubscriberRepository, SubscriberRepository>();

// Services
builder.Services.AddScoped<INewsletterService, NewsletterService.Service.Services.NewsletterService>();

// Messaging - RabbitMQ Consumer
builder.Services.AddHostedService<RabbitMqNewsletterConsumerHostedService>();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "NewsletterService",
            serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSource("NewsletterService.Messaging")
            .AddSource("NewsletterService.Service")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://tempo:4317");
            });
    });

var app = builder.Build();

app.Run();