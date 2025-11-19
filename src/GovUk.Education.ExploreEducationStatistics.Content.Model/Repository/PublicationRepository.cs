#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class PublicationRepository(ContentDbContext contentDbContext) : IPublicationRepository
{
    public async Task<bool> IsPublished(Guid publicationId, CancellationToken cancellationToken = default)
    {
        return await contentDbContext.Publications.AnyAsync(
            p => p.Id == publicationId && p.LatestPublishedReleaseVersionId != null,
            cancellationToken: cancellationToken
        );
    }

    public async Task<bool> IsSuperseded(Guid publicationId, CancellationToken cancellationToken = default)
    {
        // To be superseded, a superseding publication must exist and have a published release
        return await contentDbContext.Publications.AnyAsync(
            publication =>
                publication.Id == publicationId
                && publication.SupersededBy != null
                && publication.SupersededBy.LatestPublishedReleaseVersionId != null,
            cancellationToken: cancellationToken
        );
    }
}
