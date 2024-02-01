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
        var publication = await _contentDbContext.Publications
            .Include(p => p.LatestPublishedRelease)
            .SingleOrDefaultAsync(p => p.Id == publicationId);
        return publication?.LatestPublishedRelease;
    }

    public async Task<Release?> GetLatestReleaseVersion(Guid publicationId)
    {
        var releases = await ListLatestReleaseVersions(publicationId);
        return releases.FirstOrDefault();
    }

    public async Task<Release?> GetLatestPublishedReleaseVersion(Guid publicationId, string releaseSlug)
    {
        var releases = await _contentDbContext.Releases
            .Where(release => release.PublicationId == publicationId
                              && release.Slug == releaseSlug)
            .ToListAsync();

        return releases.SingleOrDefault(release => release.Live
                                                   // It must also be the latest version unless the later version is a draft
                                                   && !releases.Any(r =>
                                                       r.Live
                                                       && r.PreviousVersionId == release.Id
                                                       && r.Id != release.Id));
    }

    public async Task<bool> IsLatestPublishedReleaseVersion(Guid releaseId)
    {
        var release = await _contentDbContext.Releases
            .SingleOrDefaultAsync(r => r.Id == releaseId);

        if (release == null)
        {
            return false;
        }

        var publication = await _contentDbContext.Publications
            .Include(p => p.Releases)
            .SingleAsync(p => p.Id == release.PublicationId);

        return
            // Release itself must be live
            release.Live
            // It must also be the latest version unless the later version is a draft
            && !publication.Releases.Any(r =>
                r.Live
                && r.PreviousVersionId == release.Id
                && r.Id != release.Id);
    }

    public async Task<bool> IsLatestReleaseVersion(Guid releaseId)
    {
        var release = await _contentDbContext.Releases
            .SingleOrDefaultAsync(r => r.Id == releaseId);

        if (release == null)
        {
            return false;
        }

        var publication = await _contentDbContext.Publications
            .Include(p => p.Releases)
            .SingleAsync(p => p.Id == release.PublicationId);

        return !publication.Releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
    }

    // TODO EES-4336 Remove this
    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds()
    {
        var releases = await _contentDbContext.Releases
            .ToListAsync();

        return releases.Where(release => release.Live
                                         // It must also be the latest version unless the later version is a draft
                                         && !releases.Any(r =>
                                             r.Live
                                             && r.PreviousVersionId == release.Id
                                             && r.Id != release.Id))
            .Select(r => r.Id)
            .ToList();
    }

    public async Task<List<Release>> ListLatestPublishedReleaseVersions(Guid publicationId)
    {
        var releases = await _contentDbContext.Releases
            .Where(release => release.PublicationId == publicationId)
            .ToListAsync();

        return releases.Where(release => release.Live
                                         // It must also be the latest version unless the later version is a draft
                                         && !releases.Any(r =>
                                             r.Live
                                             && r.PreviousVersionId == release.Id
                                             && r.Id != release.Id))
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage)
            .ToList();
    }

    public async Task<List<Guid>> ListLatestPublishedReleaseVersionIds(Guid publicationId)
    {
        var releases = await ListLatestPublishedReleaseVersions(publicationId);
        return releases
            .Select(release => release.Id)
            .ToList();
    }

    public async Task<List<Guid>> ListLatestReleaseVersionIds(Guid publicationId)
    {
        var releases = await _contentDbContext.Releases
            .Where(release => release.PublicationId == publicationId)
            .ToListAsync();

        return releases.Where(release => !releases.Any(r => r.PreviousVersionId == release.Id && r.Id != release.Id))
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage)
            .Select(release => release.Id)
            .ToList();
    }

    public async Task<List<Release>> ListLatestReleaseVersions()
    {
        var releases = await _contentDbContext.Releases
            .ToListAsync();

        return releases.Where(release => !releases.Any(r => r.PreviousVersionId == release.Id && r.Id != release.Id))
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage)
            .ToList();
    }

    public async Task<List<Release>> ListLatestReleaseVersions(Guid publicationId)
    {
        var releases = await _contentDbContext.Releases
            .Where(release => release.PublicationId == publicationId)
            .ToListAsync();

        return releases.Where(release => !releases.Any(r => r.PreviousVersionId == release.Id && r.Id != release.Id))
            .OrderByDescending(release => release.Year)
            .ThenByDescending(release => release.TimePeriodCoverage)
            .ToList();
    }
}
