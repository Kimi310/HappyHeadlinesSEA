using NewsletterService.Service.Interfaces;
using System.Diagnostics;
using NewsletterService.DataAccess.Models;

namespace NewsletterService.Service.Services;

public class NewsletterService : INewsletterService
{
    private static readonly ActivitySource ActivitySource = new("NewsletterService.Service");
    private readonly ILogger<NewsletterService> _logger;
    // Add your subscriber repository or email service here
    // private readonly ISubscriberRepository _subscriberRepository;
    // private readonly IEmailService _emailService;

    public NewsletterService(ILogger<NewsletterService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessArticleForNewsletter(Article article)
    {
        using var activity = ActivitySource.StartActivity("ProcessArticleForNewsletter");
        activity?.SetTag("article.id", article.Id);
        activity?.SetTag("article.is_global", article.IsGlobal);
        activity?.SetTag("article.continent", article.Continent ?? "N/A");

        _logger.LogInformation(
            "Processing article {ArticleId} ({Title}) for newsletter distribution",
            article.Id,
            article.Title);

        try
        {
            List<string> targetSubscribers;

            if (article.IsGlobal)
            {
                _logger.LogInformation("Article {ArticleId} is global - sending to all subscribers", article.Id);
                targetSubscribers = await GetAllSubscribersAsync();
            }
            else
            {
                _logger.LogInformation(
                    "Article {ArticleId} is regional - sending to {Continent} subscribers only",
                    article.Id,
                    article.Continent);
                targetSubscribers = await GetSubscribersByContinent(article.Continent!);
            }

            if (!targetSubscribers.Any())
            {
                _logger.LogWarning(
                    "No subscribers found for article {ArticleId} (IsGlobal={IsGlobal}, Continent={Continent})",
                    article.Id,
                    article.IsGlobal,
                    article.Continent ?? "N/A");
                return;
            }

            // Send newsletters to subscribers
            activity?.SetTag("newsletter.subscriber_count", targetSubscribers.Count);
            await SendNewslettersAsync(article, targetSubscribers);

            _logger.LogInformation(
                "Successfully sent newsletter for article {ArticleId} to {SubscriberCount} subscribers",
                article.Id,
                targetSubscribers.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(
                ex,
                "Failed to process article {ArticleId} for newsletter",
                article.Id);
            throw;
        }
    }

    private async Task<List<string>> GetAllSubscribersAsync()
    {
        
        _logger.LogDebug("Fetching all subscribers from database");
        await Task.Delay(10);
        
        return new List<string> 
        { 
            "subscriber1@example.com", 
            "subscriber2@example.com",
            "subscriber3@example.com"
        };
    }

    private async Task<List<string>> GetSubscribersByContinent(string continent)
    {
        
        _logger.LogDebug("Fetching subscribers for continent: {Continent}", continent);
        await Task.Delay(10); 
        
        return new List<string> 
        { 
            $"subscriber-{continent.ToLower()}1@example.com",
            $"subscriber-{continent.ToLower()}2@example.com"
        };
    }

    private async Task SendNewslettersAsync(Article article, List<string> subscriberEmails)
    {
        using var activity = ActivitySource.StartActivity("SendNewsletters");
        activity?.SetTag("newsletter.count", subscriberEmails.Count);

        _logger.LogInformation(
            "Sending newsletter about '{ArticleTitle}' to {Count} subscribers",
            article.Title,
            subscriberEmails.Count);

        foreach (var email in subscriberEmails)
        {
            _logger.LogDebug("Sending newsletter to {Email} for article {ArticleId}", email, article.Id);
            await Task.Delay(5);
        }

        _logger.LogInformation("All newsletters sent successfully");
    }
}