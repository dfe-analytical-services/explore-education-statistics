#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau
{
    [Route("api")]
    [ApiController]
    [Authorize(Roles = RoleNames.BauUser)]
    public class ReleaseOrderMigrationController : ControllerBase
    {
        private readonly ContentDbContext _context;
        private readonly IPublicationCacheService _publicationCacheService;

        public ReleaseOrderMigrationController(
            ContentDbContext context,
            IPublicationCacheService publicationCacheService)
        {
            _context = context;
            _publicationCacheService = publicationCacheService;
        }

        [HttpPatch("migration/combine-ees-legacy-releases")]
        public async Task<ActionResult> MigrateReleaseOrdering(
            [FromQuery] bool dryRun = true)
        {
            var publications = await _context.Publications
                .Include(p => p.Releases.OrderBy(r => r.Published))
                .Include(p => p.LegacyReleases.OrderBy(lr => lr.Order))
                .ToListAsync();

            var publicationsUpdated = 0;
            var eesReleasesOrdered = 0;
            var legacyReleasesReordered = 0;

            foreach (var publication in publications)
            {
                if (!publication.Releases.Any() && !publication.LegacyReleases.Any()) continue;

                var currentOrder = 0;
                publication.ReleaseOrders = new();

                foreach (var legacyRelease in publication.LegacyReleases)
                {
                    publication.ReleaseOrders.Add(new()
                    {
                        ReleaseId = legacyRelease.Id,
                        Order = ++currentOrder, // Reassign counting upwards from 1 (fix any misnumbered, or starting from 0)
                        IsLegacy = true
                    });

                    legacyReleasesReordered++;
                }

                foreach (var eesRelease in publication.Releases)
                {
                    publication.ReleaseOrders.Add(new()
                    {
                        ReleaseId = eesRelease.Id,
                        Order = ++currentOrder,
                        IsLegacy = false,
                        IsDraft = !eesRelease.Published.HasValue
                    });

                    eesReleasesOrdered++;
                }

                publicationsUpdated++;
            }

            if (!dryRun)
            {
                await _context.SaveChangesAsync();

                foreach (var publication in publications)
                {
                    await _publicationCacheService.UpdatePublication(publication.Slug);
                }

                return Ok($"Database updated successfully. {publicationsUpdated} publications updated, containing a total of\n- {eesReleasesOrdered} EES releases ordered, and\n- {legacyReleasesReordered} legacy releases reordered.");
            }

            return Ok($"Dry run completed. {publicationsUpdated} publications updated, containing a total of\n- {eesReleasesOrdered} EES releases ordered, and\n- {legacyReleasesReordered} legacy releases reordered.");
        }
    }
}
