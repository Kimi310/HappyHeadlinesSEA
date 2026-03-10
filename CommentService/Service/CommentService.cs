using comment_service.Service.Interfaces;
using DataAccess.Models;
using comment_service.Service;

namespace comment_service.Service;

public class CommentService: ICommentService
{

    public Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(Guid articleId)
    {
        throw new NotImplementedException();
    }

    public Task<Comment> CreateCommentAsync(Guid articleId, string commentTex)
    {
        throw new NotImplementedException();
    }

    public Task<Comment?> GetCommentByIdAsync(Guid commentId)
    {
        throw new NotImplementedException();
    }

    public Task<Comment> UpdateCommentAsync(Guid commentId, string commentText)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteCommentAsync(Guid commentId)
    {
        throw new NotImplementedException();
    }
}