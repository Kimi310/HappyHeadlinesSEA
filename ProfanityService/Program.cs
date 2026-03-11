using ProfanityService.DataAccess;
using ProfanityService.DataAccess.Interfaces;
using ProfanityService.DataAccess.Repositories;
using ProfanityService.Service.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Profanity Service API",
        Version = "v1",
        Description = "API for checking profanity in Happy Headlines"
    });
});

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddScoped<IProfanityDbContextFactory, ProfanityDbContextFactory>();
builder.Services.AddScoped<IProfanityRepository, ProfanityRepository>();
builder.Services.AddScoped<IProfanityService, ProfanityService.Service.ProfanityService>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Profanity Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseStatusCodePages();

app.UseCors(config => 
    config.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());

app.MapControllers();

app.Run();
