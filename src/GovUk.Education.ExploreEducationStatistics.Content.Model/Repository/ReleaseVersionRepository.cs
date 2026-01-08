#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ReleaseVersionRepository(ContentDbContext contentDbContext) : IReleaseVersionRepository
{
    public async Task<ReleaseVersion?> GetLatestReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default
    )
    {
        var latestReleaseId = await contentDbContext
            .Publications.Where(p => p.Id == publicationId)
            .Select(p => p.ReleaseSeries.LatestReleaseId())
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return latestReleaseId.HasValue
            ? await contentDbContext
                .ReleaseVersions.LatestReleaseVersion(releaseId: latestReleaseId.Value)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            : null;
    }

    public async Task<ReleaseVersion?> GetLatestPublishedReleaseVersionByReleaseSlug(
        Guid publicationId,
        string releaseSlug,
        CancellationToken cancellationToken = default
    )
    {
        // There should only ever be one latest published release version with a given slug
        return await contentDbContext
            .ReleaseVersions.LatestReleaseVersions(publicationId, releaseSlug, publishedOnly: true)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> IsLatestPublishedReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await IsLatestReleaseVersion(
            releaseVersionId,
            publishedOnly: true,
            cancellationToken: cancellationToken
        );
    }

    public async Task<bool> IsLatestReleaseVersion(Guid releaseVersionId, CancellationToken cancellationToken = default)
    {
        return await IsLatestReleaseVersion(
            releaseVersionId,
            publishedOnly: false,
            cancellationToken: cancellationToken
        );
    }

    public async Task<List<Guid>> ListLatestReleaseVersionIds(
        Guid publicationId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.LatestReleaseVersions(publicationId, publishedOnly: publishedOnly)
            .Select(rv => rv.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<ReleaseVersion>> ListLatestReleaseVersions(
        Guid publicationId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var publication = await contentDbContext.Publications.SingleOrDefaultAsync(
            p => p.Id == publicationId,
            cancellationToken: cancellationToken
        );

        if (publication == null)
        {
            return [];
        }

        var publicationReleaseSeriesReleaseIds = publication.ReleaseSeries.ReleaseIds();

        var releaseIdIndexMap = publicationReleaseSeriesReleaseIds
            .Select((releaseId, index) => (releaseId, index))
            .ToDictionary(tuple => tuple.releaseId, tuple => tuple.index);

        return (
            await contentDbContext
                .ReleaseVersions.LatestReleaseVersions(publicationId, publishedOnly: publishedOnly)
                .ToListAsync(cancellationToken: cancellationToken)
        )
            .OrderBy(rv => releaseIdIndexMap[rv.ReleaseId])
            .ToList();
    }

    private async Task<bool> IsLatestReleaseVersion(
        Guid releaseVersionId,
        bool publishedOnly,
        CancellationToken cancellationToken = default
    )
    {
        var releaseVersion = await GetReleaseIdAndVersion(releaseVersionId, cancellationToken);

        if (releaseVersion == null)
        {
            return false;
        }

        return await GetMaxVersionNumber(releaseVersion.ReleaseId, publishedOnly, cancellationToken)
            == releaseVersion.Version;
    }

    private async Task<int?> GetMaxVersionNumber(
        Guid releaseId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.ReleaseId == releaseId)
            .Where(rv => !publishedOnly || rv.Published.HasValue)
            .MaxAsync(rv => (int?)rv.Version, cancellationToken: cancellationToken);
    }

    private async Task<ReleaseIdVersion?> GetReleaseIdAndVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Where(rv => rv.Id == releaseVersionId)
            .Select(rv => new ReleaseIdVersion(rv.ReleaseId, rv.Version))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private record ReleaseIdVersion(Guid ReleaseId, int Version);
}
