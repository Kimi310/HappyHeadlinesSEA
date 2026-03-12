using DraftService;
using DraftService.Data;
using DraftService.DataAccess.Interfaces;
using DraftService.DataAccess.Repositories;
using DraftService.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Grafana.Loki;


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        "http://loki:3100",
        labels: new[]
        {
            new LokiLabel { Key = "app", Value = "draftservice" }
        })
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Host.UseSerilog();

builder.Services.AddScoped<IDraftService, DraftService.Service.Services.DraftService>();
builder.Services.AddScoped<IDraftRepository, DraftRepository>();
builder.Services.AddControllers();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStatusCodePages();

app.UseCors(config => 
    config.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());

app.MapControllers();

app.Run();
