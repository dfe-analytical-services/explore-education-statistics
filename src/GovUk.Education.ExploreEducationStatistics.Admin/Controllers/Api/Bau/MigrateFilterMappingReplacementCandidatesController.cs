#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

// EES-7281: backfills DataSetMapping.ReplacementFilters/ReplacementFilterGroups/ReplacementFilterItems for rows
// created before those columns existed. Those rows' old UnmappedReplacementFilters column only ever held the
// *unclaimed* replacement filters, not the complete replacement-side catalogue the new columns need, so this
// re-derives the catalogue from the replacement file's actual filters/groups/items rather than reinterpreting the
// old column. Safe to run more than once - it always recomputes from the statistics data, it doesn't touch
// FilterMappings at all, and it should be run once after the Ees7281ReplaceFilterMappingCandidatePools migration
// has been applied and before relying on the new columns being populated.
[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class MigrateFilterMappingReplacementCandidatesController(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext
) : ControllerBase
{
    [HttpPut("migrate-filter-mapping-replacement-candidates")]
    public async Task<ActionResult> MigrateReplacementCandidates(CancellationToken cancellationToken = default)
    {
        var dataSetMappings = await contentDbContext.DataSetMappings.ToListAsync(cancellationToken);

        foreach (var mapping in dataSetMappings)
        {
            var replacementSubjectId = await contentDbContext
                .Files.Where(f => f.Id == mapping.ReplacementDataFileId && f.Type == FileType.Data)
                .Select(f => f.SubjectId!.Value)
                .SingleAsync(cancellationToken);

            var replacementFilters = await statisticsDbContext
                .Filter.AsNoTracking()
                .Include(f => f.FilterGroups)
                    .ThenInclude(fg => fg.FilterItems)
                .Where(f => f.SubjectId == replacementSubjectId)
                .ToListAsync(cancellationToken);

            mapping.ReplacementFilters = replacementFilters
                .Select(filter => new ReplacementFilter
                {
                    Id = filter.Id,
                    Label = filter.Label,
                    ColumnName = filter.Name,
                })
                .ToList();

            mapping.ReplacementFilterGroups = replacementFilters
                .SelectMany(filter =>
                    filter.FilterGroups.Select(group => new ReplacementFilterGroup
                    {
                        Id = group.Id,
                        FilterId = filter.Id,
                        Label = group.Label,
                    })
                )
                .ToList();

            mapping.ReplacementFilterItems = replacementFilters
                .SelectMany(filter => filter.FilterGroups)
                .SelectMany(group =>
                    group.FilterItems.Select(item => new ReplacementFilterItem
                    {
                        Id = item.Id,
                        FilterGroupId = group.Id,
                        Label = item.Label,
                    })
                )
                .ToList();
        }

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return Ok($"Migrated replacement filter candidates for {dataSetMappings.Count} DataSetMapping row(s)");
    }
}
