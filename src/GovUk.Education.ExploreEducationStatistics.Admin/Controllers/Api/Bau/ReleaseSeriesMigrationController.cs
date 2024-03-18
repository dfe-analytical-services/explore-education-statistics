#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class ReleaseSeriesMigrationController : ControllerBase
{
    private readonly ContentDbContext _context;
    private readonly IPublicationCacheService _publicationCacheService;
    private readonly ILogger<ReleaseSeriesMigrationController> _logger;

    public ReleaseSeriesMigrationController( ContentDbContext context,
        IPublicationCacheService publicationCacheService,
        ILogger<ReleaseSeriesMigrationController> logger)
    {
        _context = context;
        _publicationCacheService = publicationCacheService;
        _logger = logger;
    }

    public class ReleaseSeriesMigrationResult
    {
        public bool IsDryRun;
        public int NumPublicationsUpdated;
        public string UpdatedPublicationIds = string.Empty;
    }

    [HttpPatch("bau/migrate-release-series")]
    public async Task<ReleaseSeriesMigrationResult> MigrateReleaseSeries(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var publications = await _context.Publications
            .Include(p => p.ReleaseVersions)
            .ThenInclude(rv => rv.Release)
            .Include(p => p.LegacyReleases.OrderByDescending(lr => lr.Order))
            .ToListAsync(cancellationToken);

        var updatedPublicationIds = new List<Guid>();

        foreach (var publication in publications)
        {
            publication.ReleaseSeries = new List<ReleaseSeriesItem>();

            var releaseIds = publication.ReleaseVersions
                .OrderByDescending(rv => rv.Year)
                .ThenByDescending(rv => rv.TimePeriodCoverage)
                .Select(rv => rv.ReleaseId)
                .Distinct()
                .ToList();

            foreach (var releaseId in releaseIds)
            {
                publication.ReleaseSeries.Add(
                    new()
                    {
                        Id = Guid.NewGuid(),

                        ReleaseId = releaseId,

                        LegacyLinkDescription = null,
                        LegacyLinkUrl = null,
                    });
            }

            foreach (var legacyRelease in publication.LegacyReleases)
            {
                publication.ReleaseSeries.Add(new()
                {
                    Id = Guid.NewGuid(),

                    ReleaseId = null,

                    LegacyLinkDescription = legacyRelease.Description,
                    LegacyLinkUrl = legacyRelease.Url,
                });
            }

            if (dryRun)
            {
                _logger.LogInformation("Planned ReleaseSeries to add to Publication {publicationId}:\n{releaseSeries}\n\n",
                    publication.Id, publication.ReleaseSeries);
            }

            updatedPublicationIds.Add(publication.Id);
        }

        if (!dryRun)
        {
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var publication in publications)
            {
                if (publication.Live)
                {
                    _logger.LogInformation("Publication: " + publication.Slug);
                    await _publicationCacheService.UpdatePublication(publication.Slug);
                }
            }
        }

        return new ReleaseSeriesMigrationResult
        {
            IsDryRun = dryRun,
            NumPublicationsUpdated = updatedPublicationIds.Count,
            UpdatedPublicationIds = updatedPublicationIds.Select(id => id.ToString()).JoinToString(','),
        };
    }
}
