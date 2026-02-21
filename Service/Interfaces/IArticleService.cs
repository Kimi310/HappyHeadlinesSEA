using Services.Dtos.ResponseDtos;

namespace Services.Interfaces;

public interface IArticleService
{
    public Task<List<ArticleResponseDto>> GetArticles();
}