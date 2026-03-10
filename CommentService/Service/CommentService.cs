using CommentService.DataAccess.Interfaces;
using CommentService.Service.Interfaces;
using CommentService.DataAccess.Models;
using ProfanityService.Controllers.Models;

namespace CommentService.Service;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _profanityApiUrl;
    
    public CommentService(ICommentRepository commentRepository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _commentRepository = commentRepository;
        _httpClientFactory = httpClientFactory;
        _profanityApiUrl = configuration["ProfanityService:Url"] 
            ?? throw new InvalidOperationException("ProfanityService:Url configuration is missing");
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
            Content = result.FilteredText,
            WasFiltered = wasFiltered,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.CreateAsync(comment);

        return comment;
    }
    
    public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(Guid articleId)
    {
        return await _commentRepository.GetByArticleIdAsync(articleId);
    }

    public Task<bool> DeleteCommentAsync(Guid commentId)
    {
        throw new NotImplementedException();
    }
}