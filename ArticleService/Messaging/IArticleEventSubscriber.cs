using DataAccess.Models;

namespace ArticleService.Messaging;

public interface IArticleEventSubscriber
{
    Task<Article> SubscribeArticleAsync(CancellationToken cancellationToken = default);
}