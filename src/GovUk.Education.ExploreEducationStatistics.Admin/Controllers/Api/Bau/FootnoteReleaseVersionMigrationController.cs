#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

// TODO EES-2979 Remove once every Footnote has a ReleaseVersionId and the ReleaseFootnote table is dropped
[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class FootnoteReleaseVersionMigrationController(StatisticsDbContext statisticsDbContext) : ControllerBase
{
    public class SharedFootnotesResult
    {
        public int SharedFootnotes { get; set; }

        public int FootnotesWithoutReleaseVersionId { get; set; }

        public int OrphanedFootnotes { get; set; }
    }

    public class UnshareFootnotesResult
    {
        public bool IsDryRun { get; set; }

        public int ForkedFootnotes { get; set; }
    }

    public class DeleteOrphanedFootnotesResult
    {
        public bool IsDryRun { get; set; }

        public int DeletedOrphanedFootnotes { get; set; }
    }

    public class MigrationResult
    {
        public bool IsDryRun { get; set; }

        public int ReleaseVersionIdsSet { get; set; }

        public List<Guid> SkippedFootnoteIds { get; set; } = [];

        public int Remaining { get; set; }
    }

    /// <summary>
    /// Counts the Footnotes which are shared by more than one ReleaseVersion, the Footnotes which have no
    /// ReleaseVersionId, and the Footnotes which belong to no ReleaseVersion.
    /// </summary>
    [HttpGet("shared-footnotes")]
    public async Task<SharedFootnotesResult> GetSharedFootnotes(CancellationToken cancellationToken = default) =>
        new()
        {
            SharedFootnotes = await statisticsDbContext.Footnote.CountAsync(
                footnote => footnote.Releases.Count > 1,
                cancellationToken
            ),
            FootnotesWithoutReleaseVersionId = await statisticsDbContext.Footnote.CountAsync(
                footnote => footnote.ReleaseVersionId == null,
                cancellationToken
            ),
            OrphanedFootnotes = await statisticsDbContext.Footnote.CountAsync(
                footnote => !footnote.Releases.Any(),
                cancellationToken
            ),
        };

    /// <summary>
    /// Gives every Footnote shared by more than one ReleaseVersion a copy of its own for every ReleaseVersion but the
    /// first.
    /// </summary>
    [HttpPut("unshare-footnotes")]
    public async Task<UnshareFootnotesResult> UnshareFootnotes(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        // Copying footnotes doesn't change them, so they keep their Created and Updated timestamps
        statisticsDbContext.ChangeTracker.StateChanged -= DbContextUtils.UpdateTimestamps;
        statisticsDbContext.ChangeTracker.Tracked -= DbContextUtils.UpdateTimestamps;

        var sharedFootnotes = await statisticsDbContext
            .Footnote.Include(footnote => footnote.Subjects)
            .Include(footnote => footnote.Filters)
            .Include(footnote => footnote.FilterGroups)
            .Include(footnote => footnote.FilterItems)
            .Include(footnote => footnote.Indicators)
            .Include(footnote => footnote.Releases)
            .Where(footnote => footnote.Releases.Count > 1)
            .ToListAsync(cancellationToken);

        var forkedFootnotes = 0;

        foreach (var footnote in sharedFootnotes)
        {
            // The existing footnote keeps the first ReleaseFootnote, and a copy of the footnote is created for each
            // of the remaining ReleaseFootnotes
            var releaseFootnotes = footnote
                .Releases.OrderBy(releaseFootnote => releaseFootnote.ReleaseVersionId)
                .Skip(1)
                .ToList();

            foreach (var releaseFootnote in releaseFootnotes)
            {
                var fork = new Footnote
                {
                    Id = Guid.NewGuid(),
                    Content = footnote.Content,
                    Order = footnote.Order,
                    Created = footnote.Created,
                    Updated = footnote.Updated,
                    Releases = [new ReleaseFootnote { ReleaseVersionId = releaseFootnote.ReleaseVersionId }],
                    Subjects = footnote
                        .Subjects.Select(link => new SubjectFootnote { SubjectId = link.SubjectId })
                        .ToList(),
                    Filters = footnote.Filters.Select(link => new FilterFootnote { FilterId = link.FilterId }).ToList(),
                    FilterGroups = footnote
                        .FilterGroups.Select(link => new FilterGroupFootnote { FilterGroupId = link.FilterGroupId })
                        .ToList(),
                    FilterItems = footnote
                        .FilterItems.Select(link => new FilterItemFootnote { FilterItemId = link.FilterItemId })
                        .ToList(),
                    Indicators = footnote
                        .Indicators.Select(link => new IndicatorFootnote { IndicatorId = link.IndicatorId })
                        .ToList(),
                };

                statisticsDbContext.ReleaseFootnote.Remove(releaseFootnote);
                statisticsDbContext.Footnote.Add(fork);
                forkedFootnotes++;
            }
        }

        if (!dryRun)
        {
            await statisticsDbContext.SaveChangesAsync(cancellationToken);
        }

        return new UnshareFootnotesResult { IsDryRun = dryRun, ForkedFootnotes = forkedFootnotes };
    }

    /// <summary>
    /// Deletes Footnotes which belong to no ReleaseVersion.
    /// </summary>
    [HttpPut("delete-orphaned-footnotes")]
    public async Task<DeleteOrphanedFootnotesResult> DeleteOrphanedFootnotes(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        // These orphaned footnotes aren't created by this migration - but we still want to remove them so we can
        // update Footnote.ReleaseVersionId to be non-nullable later
        var orphanedFootnotes = await statisticsDbContext
            .Footnote.Where(footnote => !footnote.Releases.Any())
            .ToListAsync(cancellationToken);

        statisticsDbContext.Footnote.RemoveRange(orphanedFootnotes);

        if (!dryRun)
        {
            await statisticsDbContext.SaveChangesAsync(cancellationToken);
        }

        return new DeleteOrphanedFootnotesResult
        {
            IsDryRun = dryRun,
            DeletedOrphanedFootnotes = orphanedFootnotes.Count,
        };
    }

    /// <summary>
    /// Sets the ReleaseVersionId of up to <paramref name="num"/> Footnotes from their ReleaseFootnote. Footnotes
    /// shared by more than one ReleaseVersion are skipped until they've been unshared.
    /// </summary>
    [HttpPut("migrate-footnote-release-versions")]
    public async Task<MigrationResult> MigrateFootnoteReleaseVersions(
        [FromQuery] bool dryRun = true,
        [FromQuery] int num = 1000,
        CancellationToken cancellationToken = default
    )
    {
        // Backfilling footnotes doesn't change them, so they keep their Created and Updated timestamps
        statisticsDbContext.ChangeTracker.StateChanged -= DbContextUtils.UpdateTimestamps;
        statisticsDbContext.ChangeTracker.Tracked -= DbContextUtils.UpdateTimestamps;

        var footnotesToSet = statisticsDbContext.Footnote.Where(footnote =>
            footnote.Releases.Count == 1
            && footnote.Releases.Any(releaseFootnote => releaseFootnote.ReleaseVersionId != footnote.ReleaseVersionId)
        );

        var footnotesToSetCount = await footnotesToSet.CountAsync(cancellationToken);

        var footnotes = await footnotesToSet
            .Include(footnote => footnote.Releases)
            .OrderBy(footnote => footnote.Id)
            .Take(num)
            .ToListAsync(cancellationToken);

        var releaseVersionIdsSet = 0;
        var skippedFootnoteIds = new List<Guid>();

        foreach (var footnote in footnotes)
        {
            if (footnote.Releases.Count != 1)
            {
                skippedFootnoteIds.Add(footnote.Id);
                continue;
            }

            footnote.ReleaseVersionId = footnote.Releases[0].ReleaseVersionId;
            releaseVersionIdsSet++;
        }

        if (!dryRun)
        {
            await statisticsDbContext.SaveChangesAsync(cancellationToken);
        }

        return new MigrationResult
        {
            IsDryRun = dryRun,
            ReleaseVersionIdsSet = releaseVersionIdsSet,
            SkippedFootnoteIds = skippedFootnoteIds,
            Remaining = footnotesToSetCount - releaseVersionIdsSet,
        };
    }
}
