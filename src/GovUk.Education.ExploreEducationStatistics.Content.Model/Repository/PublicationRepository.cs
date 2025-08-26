#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class PublicationRepository : IPublicationRepository
{
    private readonly ContentDbContext _contentDbContext;

    public PublicationRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<bool> IsPublished(Guid publicationId)
    {
        return await _contentDbContext.Publications
            .AnyAsync(p => p.Id == publicationId && p.LatestPublishedReleaseVersionId != null);
    }

    public async Task<bool> IsSuperseded(Guid publicationId)
    {
        // To be superseded, a superseding publication must exist and have a published release
        return await _contentDbContext
            .Publications
            .Include(publication => publication.SupersededBy)
            .AnyAsync(publication => publication.Id == publicationId &&
                                     publication.SupersededBy != null &&
                                     publication.SupersededBy.LatestPublishedReleaseVersionId != null);
    }
}
