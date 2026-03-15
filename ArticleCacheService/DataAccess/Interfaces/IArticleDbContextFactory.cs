namespace ArticleCacheService.DataAccess.Interfaces;

public interface IArticleDbContextFactory
{
    ArticleDbContext Create(string continent, bool isGlobal);
}

