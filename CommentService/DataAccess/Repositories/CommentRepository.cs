using CommentService.DataAccess.Interfaces;
using CommentService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CommentService.DataAccess.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ICommentDbContextFactory _factory;

    public CommentRepository(ICommentDbContextFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<Comment> CreateAsync(Comment comment)
    {
        var context = _factory.Create(true);
        var response = context.Comments.Add(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public Task<IEnumerable<Comment>> GetByArticleIdAsync(Guid articleId)
    {
        var context = _factory.Create(true);
        var response = context.Comments.Where(c => c.ArticleId == articleId);
        return Task.FromResult(response.AsEnumerable());
    }

    public Task<Comment?> GetByIdAsync(Guid commentId)
    {
        var context = _factory.Create(true);
        var response = context.Comments.FirstOrDefault(c => c.Id == commentId);
        return Task.FromResult(response);
    }

    public async Task<Comment?> UpdateAsync(Guid commentId, string commentText)
    {
        var context = _factory.Create(true);
        var response =  await context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        return response;
    }

    public async Task DeleteAsync(Comment comment)
    {
        var context = _factory.Create(true);
        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
    }
}