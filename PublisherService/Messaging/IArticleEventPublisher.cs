using DataAccess.Models;

namespace PublisherService.Messaging;

public interface IArticleEventPublisher
{
    Task PublishArticleAsync(Article article, CancellationToken cancellationToken = default);
}

