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

public class ReleaseRepository : IReleaseRepository
{
    private readonly ContentDbContext _contentDbContext;

    public ReleaseRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<DateTime> GetPublishedDate(Guid releaseId,
        DateTime actualPublishedDate)
    {
        var release = await _contentDbContext.Releases
            .SingleAsync(r => r.Id == releaseId);

        // If the release is already published return the published date
        if (release.Published.HasValue)
        {
            return release.Published.Value;
        }

        // For the first version of a release or if an update to the published date has been requested
        // return the actual published date
        if (release.Version == 0 || release.UpdatePublishedDate)
        {
            return actualPublishedDate;
        }

        // Otherwise, return the published date from the previous version
        await _contentDbContext.Entry(release)
            .Reference(r => r.PreviousVersion)
            .LoadAsync();

        if (!release.PreviousVersion!.Published.HasValue)
        {
            throw new ArgumentException(
                $"Expected Release {release.PreviousVersionId} to have a Published date as the previous version of Release {release.Id}");
        }

        return release.PreviousVersion.Published.Value;
    }

    public async Task<Release?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.Releases.LatestReleaseVersions(publicationId, publishedOnly: true)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .FirstOrDefault();
    }

    public async Task<Release?> GetLatestReleaseVersion(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.Releases.LatestReleaseVersions(publicationId)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .FirstOrDefault();
    }

    public async Task<Release?> GetLatestPublishedReleaseVersion(
        Guid publicationId,
        string releaseSlug,
        CancellationToken cancellationToken = default)
    {
        // There should only ever be one latest published release version with a given slug
        return await _contentDbContext.Releases.LatestReleaseVersions(publicationId, releaseSlug, publishedOnly: true)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> IsLatestPublishedReleaseVersion(
        Guid releaseId,
        CancellationToken cancellationToken = default)
    {
        return await IsLatestReleaseVersion(releaseId,
            publishedOnly: true,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> IsLatestReleaseVersion(
        Guid releaseId,
        CancellationToken cancellationToken = default)
    {
        return await IsLatestReleaseVersion(releaseId,
            publishedOnly: false,
            cancellationToken: cancellationToken);
    }

    // TODO EES-4336 Remove this
    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds(CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.Releases.LatestReleaseVersions(publishedOnly: true)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Release>> ListLatestPublishedReleaseVersions(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.Releases.LatestReleaseVersions(publicationId, publishedOnly: true)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.Releases.LatestReleaseVersions(publicationId, publishedOnly: true)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Guid>> ListLatestReleaseVersionIds(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.Releases.LatestReleaseVersions(publicationId)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Release>> ListLatestReleaseVersions(CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.Releases.LatestReleaseVersions()
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    public async Task<List<Release>> ListLatestReleaseVersions(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return (await _contentDbContext.Releases.LatestReleaseVersions(publicationId)
                .ToListAsync(cancellationToken: cancellationToken))
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    private async Task<bool> IsLatestReleaseVersion(
        Guid releaseId,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var release = await GetReleaseParentIdAndVersion(releaseId, cancellationToken);

        if (release == null)
        {
            return false;
        }

        return await GetMaxVersionNumber(release.ReleaseParentId, publishedOnly, cancellationToken) == release.Version;
    }

    private async Task<int?> GetMaxVersionNumber(
        Guid releaseParentId,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.Releases
            .Where(r => r.ReleaseParentId == releaseParentId)
            .Where(r => !publishedOnly || r.Published.HasValue)
            .MaxAsync(r => (int?) r.Version,
                cancellationToken: cancellationToken);
    }

    private async Task<ParentIdVersion?> GetReleaseParentIdAndVersion(
        Guid releaseId,
        CancellationToken cancellationToken = default)
    {
        return await _contentDbContext.Releases
            .Where(r => r.Id == releaseId)
            .Select(r => new ParentIdVersion(r.ReleaseParentId, r.Version))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private record ParentIdVersion(Guid ReleaseParentId, int Version);
}

internal static class ReleaseIEnumerableExtensions
{
    internal static IOrderedEnumerable<Release> OrderByReverseChronologicalOrder(
        this IEnumerable<Release> query)
    {
        return query.OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage);
    }
}
