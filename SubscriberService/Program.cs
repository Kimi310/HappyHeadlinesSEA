using SubscriberService.DataAccess;
using SubscriberService.DataAccess.Interfaces;
using SubscriberService.DataAccess.Repositories;
using SubscriberService.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddScoped<ISubscriberDbContextFactory, SubscriberDbContextFactory>();
builder.Services.AddScoped<ISubscriberRepository, SubscriberRepository>();
builder.Services.AddScoped<ISubscriberService, SubscriberService.Service.SubscriberService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
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