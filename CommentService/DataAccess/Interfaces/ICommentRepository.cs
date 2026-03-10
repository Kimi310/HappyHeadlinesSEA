using CommentService.DataAccess.Models;
using CommentService.DataAccess.Models;

namespace CommentService.DataAccess.Interfaces;

public interface ICommentRepository
{
    // Stores a comment on an article after filtering out profanity.
    public Task<Comment> CreateAsync(Comment comment);
    
    // Fetches all comments for a specific article.
    public Task<IEnumerable<Comment>> GetByArticleIdAsync(Guid articleId);
    
    // Fetches a specific comment by its ID.
    public Task<Comment?> GetByIdAsync(Guid commentId);
    
    // Updates an existing comment after filtering profanity.
    public Task<Comment?> UpdateAsync(Guid commentId, string commentText);
    
    // Deletes a comment from an article.
    public Task DeleteAsync(Comment comment);
}