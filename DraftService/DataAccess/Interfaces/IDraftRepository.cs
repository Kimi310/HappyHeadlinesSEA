using DraftService.Models;

namespace DraftService.DataAccess.Interfaces;

public interface IDraftRepository
{
    public Task<List<Draft>> GetDrafts();
    
    public Task AddDraft(Draft draft);
}