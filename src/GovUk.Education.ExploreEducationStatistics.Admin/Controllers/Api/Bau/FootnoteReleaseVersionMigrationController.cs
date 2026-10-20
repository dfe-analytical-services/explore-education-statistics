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
    public class MigrationResult
    {
        public bool IsDryRun { get; set; }

        public int ForkedFootnotes { get; set; }

        public int DeletedOrphanedFootnotes { get; set; }

        public int ReleaseVersionIdsSet { get; set; }

        public int Remaining { get; set; }
    }

    public class SharedFootnotesResult
    {
        public int SharedFootnotes { get; set; }

        public int FootnotesWithoutReleaseVersionId { get; set; }
    }

    /// <summary>
    /// Counts the Footnotes which are shared by more than one ReleaseVersion, and the Footnotes which have no
    /// ReleaseVersionId.
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
        };

    /// <summary>
    /// Gives every Footnote shared by more than one ReleaseVersion a copy of its own for every ReleaseVersion but the
    /// first, deletes Footnotes which belong to no ReleaseVersion, and sets the ReleaseVersionId of up to
    /// <paramref name="num"/> Footnotes from their ReleaseFootnote.
    /// </summary>
    [HttpPut("migrate-footnote-release-versions")]
    public async Task<MigrationResult> MigrateFootnoteReleaseVersions(
        [FromQuery] bool dryRun = true,
        [FromQuery] int num = 1000,
        CancellationToken cancellationToken = default
    )
    {
        // Copying and backfilling footnotes doesn't change them, so they keep their Created and Updated timestamps
        statisticsDbContext.ChangeTracker.StateChanged -= DbContextUtils.UpdateTimestamps;
        statisticsDbContext.ChangeTracker.Tracked -= DbContextUtils.UpdateTimestamps;

        var result = new MigrationResult { IsDryRun = dryRun };

        await ForkSharedFootnotes(result, cancellationToken);
        await DeleteOrphanedFootnotes(result, cancellationToken);
        await SetReleaseVersionIds(result, num, cancellationToken);

        if (!dryRun)
        {
            await statisticsDbContext.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private async Task ForkSharedFootnotes(MigrationResult result, CancellationToken cancellationToken)
    {
        var sharedFootnotes = await statisticsDbContext
            .Footnote.Include(footnote => footnote.Subjects)
            .Include(footnote => footnote.Filters)
            .Include(footnote => footnote.FilterGroups)
            .Include(footnote => footnote.FilterItems)
            .Include(footnote => footnote.Indicators)
            .Include(footnote => footnote.Releases)
            .Where(footnote => footnote.Releases.Count > 1)
            .ToListAsync(cancellationToken);

        foreach (var footnote in sharedFootnotes)
        {
            var releaseFootnotes = footnote
                .Releases.OrderBy(releaseFootnote => releaseFootnote.ReleaseVersionId)
                .ToList();

            // The existing footnote takes the first ReleaseFootnote ReleaseVersionId, and a copy of the footnote is
            // created for each of the remaining ReleaseFootnotes ReleaseVersionIds
            var firstReleaseFootnote = releaseFootnotes[0];
            footnote.ReleaseVersionId = firstReleaseFootnote.ReleaseVersionId;
            result.ReleaseVersionIdsSet++;

            foreach (var releaseFootnote in releaseFootnotes.Skip(1))
            {
                var fork = new Footnote
                {
                    Id = Guid.NewGuid(),
                    Content = footnote.Content,
                    Order = footnote.Order,
                    Created = footnote.Created,
                    Updated = footnote.Updated,
                    ReleaseVersionId = releaseFootnote.ReleaseVersionId,
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
                result.ForkedFootnotes++;
            }
        }
    }

    private async Task DeleteOrphanedFootnotes(MigrationResult result, CancellationToken cancellationToken)
    {
        // These orphaned footnotes aren't created by this migration. They already exist, left without a
        // ReleaseFootnote when deleting a Subject removed their only link to a ReleaseVersion, or by statistics
        // ReleaseVersion deletions before EES-7369. Nothing can reach them, but they have no ReleaseVersion to set
        // ReleaseVersionId from, so they need removing before ReleaseVersionId can be made non-nullable. Any with a
        // ReleaseVersionId would also reappear in that ReleaseVersion once footnotes are read through
        // ReleaseVersionId rather than ReleaseFootnote.
        var orphanedFootnotes = await statisticsDbContext
            .Footnote.Where(footnote => !footnote.Releases.Any())
            .ToListAsync(cancellationToken);

        statisticsDbContext.Footnote.RemoveRange(orphanedFootnotes);
        result.DeletedOrphanedFootnotes = orphanedFootnotes.Count;
    }

    private async Task SetReleaseVersionIds(MigrationResult result, int num, CancellationToken cancellationToken)
    {
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

        foreach (var footnote in footnotes)
        {
            footnote.ReleaseVersionId = footnote.Releases[0].ReleaseVersionId;
        }

        result.ReleaseVersionIdsSet += footnotes.Count;
        result.Remaining = footnotesToSetCount - footnotes.Count;
    }
}
