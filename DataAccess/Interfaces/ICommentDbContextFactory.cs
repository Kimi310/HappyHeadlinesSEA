namespace DataAccess.Interfaces;

public interface ICommentDbContextFactory
{
    CommentDbContext Create(bool isGlobal);
}