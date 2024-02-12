#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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

    public async Task<Release?> GetLatestPublishedReleaseVersion(Guid publicationId)
    {
        return (await GetLatestReleaseVersionsQueryable(publicationId, publishedOnly: true)
                .ToListAsync())
            .OrderByReverseChronologicalOrder()
            .FirstOrDefault();
    }

    public async Task<Release?> GetLatestReleaseVersion(Guid publicationId)
    {
        return (await GetLatestReleaseVersionsQueryable(publicationId)
                .ToListAsync())
            .OrderByReverseChronologicalOrder()
            .FirstOrDefault();
    }

    public async Task<Release?> GetLatestPublishedReleaseVersion(Guid publicationId, string releaseSlug)
    {
        // There should only ever be one latest published release version with a given slug
        return await GetLatestReleaseVersionsQueryable(publicationId, releaseSlug, publishedOnly: true)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsLatestPublishedReleaseVersion(Guid releaseId)
    {
        return await IsLatestReleaseVersion(releaseId, publishedOnly: true);
    }

    public async Task<bool> IsLatestReleaseVersion(Guid releaseId)
    {
        return await IsLatestReleaseVersion(releaseId, publishedOnly: false);
    }

    // TODO EES-4336 Remove this
    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds()
    {
        return await GetLatestReleaseVersionIdsQueryable(publishedOnly: true)
            .ToListAsync();
    }

    public async Task<List<Release>> ListLatestPublishedReleaseVersions(Guid publicationId)
    {
        return (await GetLatestReleaseVersionsQueryable(publicationId, publishedOnly: true)
                .ToListAsync())
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds(Guid publicationId)
    {
        return await GetLatestReleaseVersionIdsQueryable(publicationId, publishedOnly: true)
            .ToListAsync();
    }

    public async Task<List<Guid>> ListLatestReleaseVersionIds(Guid publicationId)
    {
        return await GetLatestReleaseVersionIdsQueryable(publicationId)
            .ToListAsync();
    }

    public async Task<List<Release>> ListLatestReleaseVersions()
    {
        return (await GetLatestReleaseVersionsQueryable()
                .ToListAsync())
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    public async Task<List<Release>> ListLatestReleaseVersions(Guid publicationId)
    {
        return (await GetLatestReleaseVersionsQueryable(publicationId)
                .ToListAsync())
            .OrderByReverseChronologicalOrder()
            .ToList();
    }

    private IQueryable<Guid> GetLatestReleaseVersionIdsQueryable(Guid? publicationId = null,
        bool publishedOnly = false)
    {
        return GetLatestReleaseVersionsQueryable(publicationId, publishedOnly: publishedOnly)
            .Select(release => release.Id);
    }

    private IQueryable<Release> GetLatestReleaseVersionsQueryable(Guid? publicationId = null,
        string? releaseSlug = null,
        bool publishedOnly = false)
    {
        if (releaseSlug != null && publicationId == null)
        {
            throw new ArgumentException("Cannot filter by release slug without a publication id");
        }

        var maxVersionsQueryable = _contentDbContext.Releases
            .Where(release => publicationId == null || release.PublicationId == publicationId)
            .Where(release => releaseSlug == null || release.Slug == releaseSlug)
            .Where(release => !publishedOnly || release.Published.HasValue)
            .GroupBy(release => release.ReleaseParentId)
            .Select(groupedReleases =>
                new
                {
                    ReleaseParentId = groupedReleases.Key, Version = groupedReleases.Max(release => release.Version)
                });

        return _contentDbContext.Releases
            .Join(maxVersionsQueryable,
                release => new { release.ReleaseParentId, release.Version },
                maxVersion => maxVersion,
                (release, _) => release);
    }

    private async Task<bool> IsLatestReleaseVersion(Guid releaseId, bool publishedOnly)
    {
        var release = await GetReleaseParentIdAndVersion(releaseId);

        if (release == null)
        {
            return false;
        }

        return await GetMaxVersionNumber(release.ReleaseParentId, publishedOnly) == release.Version;
    }

    private async Task<int?> GetMaxVersionNumber(Guid releaseParentId, bool publishedOnly = false)
    {
        return await _contentDbContext.Releases
            .Where(r => r.ReleaseParentId == releaseParentId)
            .Where(r => !publishedOnly || r.Published.HasValue)
            .MaxAsync(r => (int?) r.Version);
    }

    private async Task<ParentIdVersion?> GetReleaseParentIdAndVersion(Guid releaseId)
    {
        return await _contentDbContext.Releases
            .Where(r => r.Id == releaseId)
            .Select(r => new ParentIdVersion(r.ReleaseParentId, r.Version))
            .FirstOrDefaultAsync();
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
