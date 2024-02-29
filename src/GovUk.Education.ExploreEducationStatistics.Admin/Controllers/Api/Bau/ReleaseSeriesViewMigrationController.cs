#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class ReleaseSeriesViewMigrationController : ControllerBase
{
    private readonly ContentDbContext _context;
    private readonly IPublicationCacheService _publicationCacheService;

    public ReleaseSeriesViewMigrationController(
        ContentDbContext context,
        IPublicationCacheService publicationCacheService)
    {
        _context = context;
        _publicationCacheService = publicationCacheService;
    }

    [HttpPatch("bau/migrate-release-series-view")] // @MarkFix update this after you've done with everything else
    public async Task<ActionResult> MigrateReleaseSeriesView(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        // @MarkFix sort this function out

        var publications = await _context.Publications
            .Include(p => p.Releases)
            .Include(p => p.LegacyReleases.OrderBy(lr => lr.Order))
            .ToListAsync(cancellationToken);

        var publicationsUpdated = 0;
        var eesReleasesOrdered = 0;
        var legacyReleasesReordered = 0;

        foreach (var publication in publications)
        {
            if (!publication.Releases.Any() && !publication.LegacyReleases.Any()) continue;

            var currentOrder = 0;
            publication.ReleaseSeriesView = new();

            foreach (var legacyRelease in publication.LegacyReleases)
            {
                //publication.ReleaseSeriesView.Add(new() // @MarkFix
                //{
                //    ReleaseId = legacyRelease.Id,
                //    Order = ++currentOrder, // Reassign counting upwards from 1 (fix any misnumbered, or starting from 0)
                //    IsLegacy = true
                //});

                legacyReleasesReordered++;
            }

            // Get a list of original releases
            var releases = publication.Releases
                .OrderByDescending(release => release.Year)
                .ThenByDescending(release => release.TimePeriodCoverage);

            // Get the latest version of each release
            var latestReleaseVersions = releases
                .GroupBy(r => r.ReleaseName)
                .Select(grouping => grouping
                    .OrderByDescending(r => r.Version)
                    .First())
                .ToList();

            foreach (var latestRelease in latestReleaseVersions)
            {
                if (latestRelease.Amendment && latestRelease.PreviousVersionId.HasValue)
                {
                    // Add a ReleaseSeriesItem for the original
                    var originalRelease = releases.First(r => r.Id == latestRelease.PreviousVersionId);

                    //publication.ReleaseSeriesView.Add( // @MarkFix
                    //    new()
                    //    {
                    //        ReleaseId = originalRelease.Id,
                    //        Order = ++currentOrder,
                    //        IsDraft = !originalRelease.Published.HasValue,
                    //        IsAmendment = originalRelease.Amendment
                    //    });

                    // Followed by a ReleaseSeriesItem for the amendment, with the same Order
                    //publication.ReleaseSeriesView.Add( // @MarkFix
                    //    new()
                    //    {
                    //        ReleaseId = latestRelease.Id,
                    //        Order = currentOrder,
                    //        IsDraft = true,
                    //        IsAmendment = true
                    //    });

                    eesReleasesOrdered += 2;
                }
                else
                {
                    // The release is the only active version, so just add a single ReleaseSeriesItem
                    //publication.ReleaseSeriesView.Add( // @MarkFix
                    //    new()
                    //    {
                    //        ReleaseId = latestRelease.Id,
                    //        Order = ++currentOrder,
                    //        IsDraft = !latestRelease.Published.HasValue,
                    //    });

                    eesReleasesOrdered++;
                }
            }

            publicationsUpdated++;
        }

        if (!dryRun)
        {
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var publication in publications)
            {
                await _publicationCacheService.UpdatePublication(publication.Slug);
            }

            return Ok($"Database updated successfully. {publicationsUpdated} publications updated, containing a total of\n- {eesReleasesOrdered} EES releases ordered, and\n- {legacyReleasesReordered} legacy releases reordered.");
        }

        return Ok($"Dry run completed. {publicationsUpdated} publications updated, containing a total of\n- {eesReleasesOrdered} EES releases ordered, and\n- {legacyReleasesReordered} legacy releases reordered.");
    }
}
