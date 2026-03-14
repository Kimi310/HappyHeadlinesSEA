using NewsletterService.DataAccess.Models;

namespace NewsletterService.Service.Interfaces;

public interface INewsletterService
{
    /// <summary>
    /// Processes a newly published article for immediate newsletter distribution.
    /// Sends newsletters to subscribers based on article's global flag and continent.
    /// </summary>
    /// <param name="article">The article to process for newsletter</param>
    Task ProcessArticleForNewsletter(Article article);
}