using CommentService.DataAccess;
using CommentService.DataAccess.Interfaces;
using CommentService.DataAccess.Repositories;
using CommentService.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddScoped<ICommentDbContextFactory, CommentDbContextFactory>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService.Service.CommentService>();
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