#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ReleaseRepository(ContentDbContext contentDbContext) : IReleaseRepository
{
    public async Task<List<Release>> ListPublishedReleases(
        Guid publicationId,
        CancellationToken cancellationToken = default
    )
    {
        var publication = await contentDbContext.Publications.SingleAsync(
            p => p.Id == publicationId,
            cancellationToken: cancellationToken
        );

        var publicationReleaseSeriesReleaseIds = publication.ReleaseSeries.ReleaseIds();

        var releaseIdIndexMap = publicationReleaseSeriesReleaseIds
            .Select((releaseId, index) => (releaseId, index))
            .ToDictionary(tuple => tuple.releaseId, tuple => tuple.index);

        return (await QueryPublishedReleases(publicationId).ToListAsync(cancellationToken))
            .OrderBy(r => releaseIdIndexMap[r.Id])
            .ToList();
    }

    public async Task<List<Guid>> ListPublishedReleaseIds(
        Guid publicationId,
        CancellationToken cancellationToken = default
    )
    {
        return await QueryPublishedReleases(publicationId).Select(r => r.Id).ToListAsync(cancellationToken);
    }

    private IQueryable<Release> QueryPublishedReleases(Guid publicationId)
    {
        // For simplicity, we only query releases that have ANY version that has been published.
        // In future this may need to change if release versions can be recalled/unpublished.
        return contentDbContext
            .Releases.Where(r => r.PublicationId == publicationId)
            .Where(r => r.Versions.Any(rv => rv.Published != null));
    }
}
