using ArticleQueue.Health;
using ArticleQueue.Options;
using ArticleQueue.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<BrokerRuntimeState>();
builder.Services.AddHostedService<QueueTopologyHostedService>();

builder.Services.AddHealthChecks()
    .AddCheck<BrokerHealthCheck>("rabbitmq_topology");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", (IOptions<RabbitMqOptions> options) =>
{
    var rabbitMq = options.Value;
    return Results.Ok(new
    {
        service = "ArticleQueue",
        rabbitMqHost = rabbitMq.Host,
        rabbitMqPort = rabbitMq.Port,
        exchange = rabbitMq.Exchange,
        queue = rabbitMq.Queue,
        routingKey = rabbitMq.RoutingKey
    });
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Name == "rabbitmq_topology"
});

app.Run();

