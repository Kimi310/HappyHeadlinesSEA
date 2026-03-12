using DraftService.Models;

namespace DraftService.Service.Interfaces;

public interface IDraftService
{
    public Task<List<Draft>> GetDrafts();
    
    public Task AddDraft(Draft draft);
}