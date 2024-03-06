#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ReleaseVersionRepository : IReleaseVersionRepository
{
    private readonly ContentDbContext _contentDbContext;

    public ReleaseVersionRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<DateTime> GetPublishedDate(Guid releaseVersionId,
        DateTime actualPublishedDate)
    {
        var releaseVersion = await _contentDbContext.ReleaseVersions
            .SingleAsync(rv => rv.Id == releaseVersionId);

        // If the release version is already published return the published date
        if (releaseVersion.Published.HasValue)
        {
            return releaseVersion.Published.Value;
        }

        // For the first version of a release or if an update to the published date has been requested
        // return the actual published date
        if (releaseVersion.Version == 0 || releaseVersion.UpdatePublishedDate)
        {
            return actualPublishedDate;
        }

        // Otherwise, return the published date from the previous version
        await _contentDbContext.Entry(releaseVersion)
            .Reference(rv => rv.PreviousVersion)
            .LoadAsync();

        if (!releaseVersion.PreviousVersion!.Published.HasValue)
        {
            throw new ArgumentException(
                $"Expected Release {releaseVersion.PreviousVersionId} to have a Published date as the previous version of Release {releaseVersion.Id}");
        }

        return releaseVersion.PreviousVersion.Published.Value;
    }

    public async Task<ReleaseVersion?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId, publishedOnly: true)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .FirstOrDefault();
    }

    public async Task<ReleaseVersion?> GetLatestReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .FirstOrDefault();
    }

    public async Task<ReleaseVersion?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        string releaseSlug,
        CancellationToken cancellationToken = default)
    {
        // There should only ever be one latest published release version with a given slug
        return await _contentDbContext.ReleaseVersions
            .LatestReleaseVersions(publicationId, releaseSlug, publishedOnly: true)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> IsLatestPublishedReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await IsLatestReleaseVersion(releaseVersionId,
            publishedOnly: true,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> IsLatestReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await IsLatestReleaseVersion(releaseVersionId,
            publishedOnly: false,
            cancellationToken: cancellationToken);
    }

    // TODO EES-4336 Remove this
    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds(CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publishedOnly: true)
            .Select(rv => rv.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<ReleaseVersion>> ListLatestPublishedReleaseVersions(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId, publishedOnly: true)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId, publishedOnly: true)
            .Select(rv => rv.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Guid>> ListLatestReleaseVersionIds(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId)
            .Select(rv => rv.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<ReleaseVersion>> ListLatestReleaseVersions(CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.ReleaseVersions.LatestReleaseVersions()
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    public async Task<List<ReleaseVersion>> ListLatestReleaseVersions(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.ReleaseVersions.LatestReleaseVersions(publicationId)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    private async Task<bool> IsLatestReleaseVersion(
        Guid releaseVersionId,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var releaseVersion = await GetReleaseIdAndVersion(releaseVersionId, cancellationToken);

        if (releaseVersion == null)
        {
            return false;
        }

        return await GetMaxVersionNumber(releaseVersion.ReleaseId, publishedOnly, cancellationToken) ==
               releaseVersion.Version;
    }

    private async Task<int?> GetMaxVersionNumber(
        Guid releaseId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.ReleaseVersions
            .Where(rv => rv.ReleaseId == releaseId)
            .Where(rv => !publishedOnly || rv.Published.HasValue)
            .MaxAsync(rv => (int?)rv.Version,
                cancellationToken: cancellationToken);
    }

    private async Task<ReleaseIdVersion?> GetReleaseIdAndVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.ReleaseVersions
            .Where(rv => rv.Id == releaseVersionId)
            .Select(rv => new ReleaseIdVersion(rv.ReleaseId, rv.Version))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private record ReleaseIdVersion(Guid ReleaseId, int Version);
}

internal static class ReleaseVersionIEnumerableExtensions
{
    internal static IOrderedEnumerable<ReleaseVersion> OrderByReverseChronologicalOrder(
        this IEnumerable<ReleaseVersion> query)
    {
        return query.OrderByDescending(releaseVersion => releaseVersion.Year)
            .ThenByDescending(releaseVersion => releaseVersion.TimePeriodCoverage);
    }
}
