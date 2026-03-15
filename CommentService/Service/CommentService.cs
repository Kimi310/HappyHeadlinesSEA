using CommentService.DataAccess.Interfaces;
using CommentService.Service.Interfaces;
using CommentService.DataAccess.Models;

namespace CommentService.Service;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CommentService> _logger;
    private readonly string _profanityApiUrl;
    private readonly string _commentCacheUrl;
    
    public CommentService(
        ICommentRepository commentRepository, 
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _profanityApiUrl = configuration["ProfanityService:Url"] 
            ?? throw new InvalidOperationException("ProfanityService:Url configuration is missing");
        _commentCacheUrl = configuration["CommentCache:Url"] 
            ?? "http://comment-cache:80";
    }
    
    public async Task<Comment> CreateCommentAsync(Guid articleId, string commentText)
    {
        if (string.IsNullOrWhiteSpace(commentText))
        {
            throw new ArgumentException("Comment text cannot be empty", nameof(commentText));
        }

        var httpClient = _httpClientFactory.CreateClient();
    
        var requestBody = new
        {
            text = commentText,
            replacementChar = '*'
        };

        var response = await httpClient.PostAsJsonAsync($"{_profanityApiUrl}/api/profanity/filter", requestBody);
        response.EnsureSuccessStatusCode();
    
        var result = await response.Content.ReadFromJsonAsync<FilterResponse>();
    
        var wasFiltered = result.FilteredText != commentText;

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ArticleId = articleId,
            Content = wasFiltered ? result.FilteredText : commentText,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.CreateAsync(comment);

        return comment;
    }
    
    public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(Guid articleId)
    {
        var isTracked = await IsArticleTrackedAsync(articleId);
        
        if (!isTracked)
        {
            _logger.LogDebug("Article {ArticleId} not in tracked list - loading from database", articleId);
            return await _commentRepository.GetByArticleIdAsync(articleId);
        }

        _logger.LogInformation("Article {ArticleId} is tracked - attempting cache lookup", articleId);
        return await _commentRepository.GetByArticleIdAsync(articleId);
    }

    public Task<bool> DeleteCommentAsync(Guid commentId)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> IsArticleTrackedAsync(Guid articleId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(4);
            
            var response = await httpClient.GetAsync($"{_commentCacheUrl}/tracked-articles");
            
            if (response.IsSuccessStatusCode)
            {
                var trackedArticles = await response.Content.ReadFromJsonAsync<List<Guid>>();
                var isTracked = trackedArticles?.Contains(articleId) ?? false;
                
                _logger.LogDebug("Article {ArticleId} tracked status: {IsTracked}", articleId, isTracked);
                return isTracked;
            }

            _logger.LogWarning("CommentCache service returned {StatusCode} - falling back to normal DB access", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if article {ArticleId} is tracked - falling back to normal DB access", articleId);
            return false;
        }
    }
}