using CommentService.DataAccess.Models;

namespace CommentService.Service.Interfaces;

public interface ICommentService
{
    // Stores a comment on an article after filtering out profanity.
    Task<Comment> CreateCommentAsync(Guid articleId, string commentTex);
    
    // Fetches all comments for a specific article.
    Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(Guid articleId);
    
    // Fetches a specific comment by its ID.
    Task<Comment?> GetCommentByIdAsync(Guid commentId);
    
    // Updates an existing comment after filtering profanity.
    Task<Comment> UpdateCommentAsync(Guid commentId, string commentText);
    
    // Deletes a comment from an article.
    Task<bool> DeleteCommentAsync(Guid commentId);
}

