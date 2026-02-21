using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddScoped<IArticleDbContextFactory, ArticleDbContextFactory>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi();
app.UseStatusCodePages();

app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.MapControllers();

app.Run();

