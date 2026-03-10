using CommentService.DataAccess;

namespace CommentService.DataAccess.Interfaces;

public interface ICommentDbContextFactory
{
    CommentDbContext Create(bool isGlobal);
}