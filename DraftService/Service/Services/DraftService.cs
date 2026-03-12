using DraftService.Data;
using DraftService.DataAccess.Interfaces;
using DraftService.Models;
using DraftService.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Service.Services;

public class DraftService(IDraftRepository draftRepository, ILogger<DraftService> logger) : IDraftService
{

    public async Task<List<Draft>> GetDrafts()
    {
        logger.LogInformation("Fetching all drafts");

        try
        {
            var drafts = await draftRepository.GetDrafts();

            logger.LogInformation("Fetched {DraftCount} drafts", drafts.Count);

            return drafts;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch drafts from repository");
            throw;
        }
    }

    public async Task AddDraft(Draft draft)
    {
        logger.LogInformation("Attempting to add draft with title {DraftTitle}", draft.Title);

        try
        {
            await draftRepository.AddDraft(draft);

            logger.LogInformation("Draft added successfully with title {DraftTitle}", draft.Title);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error occurred while adding draft {DraftTitle}", draft.Title);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while adding draft {DraftTitle}", draft.Title);
            throw;
        }
    }
}