using DraftService.Data;
using DraftService.DataAccess.Interfaces;
using DraftService.Models;
using Microsoft.EntityFrameworkCore;

namespace DraftService.DataAccess.Repositories;

public class DraftRepository(AppDbContext context) : IDraftRepository
{
    public async Task AddDraft(Draft draft)
    {
        context.Drafts.Add(draft);
        await context.SaveChangesAsync();
        return ;
    }

    public async Task<List<Draft>> GetDrafts()
    {
        return await context.Drafts.ToListAsync();
    }

}