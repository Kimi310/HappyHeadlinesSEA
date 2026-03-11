using DraftService.Data;
using DraftService.DataAccess.Interfaces;
using DraftService.Models;
using DraftService.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Service.Services;

public class DraftService(IDraftRepository draftRepository) : IDraftService
{
    public Task<List<Draft>> GetDrafts()
    {
        return draftRepository.GetDrafts();
    }

    public async Task AddDraft(Draft draft)
    {
        await draftRepository.AddDraft(draft);
    }
}