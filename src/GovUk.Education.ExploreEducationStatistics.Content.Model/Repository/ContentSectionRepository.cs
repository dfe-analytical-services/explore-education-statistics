#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ContentSectionRepository : IContentSectionRepository
{
    private readonly ContentDbContext _contentDbContext;

    public ContentSectionRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<List<T>> GetAllContentBlocks<T>(Guid releaseVersionId) where T : ContentBlock
    {
        return await _contentDbContext
            .ContentBlocks
            .Where(block => block.ReleaseVersionId == releaseVersionId)
            .OfType<T>()
            .ToListAsync();
    }
}
