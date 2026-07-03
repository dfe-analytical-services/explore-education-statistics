#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class ValidateIndicatorSequencesController(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext
) : ControllerBase
{
    [HttpGet("validate-indicator-sequences")]
    public async Task<ActionResult> ValidateIndicatorSequences(CancellationToken cancellationToken = default)
    {
        var subjectIdsAndSequences = await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Where(rf => rf.IndicatorSequence != null && rf.File.SubjectId != null)
            .Select(rf => new { SubjectId = rf.File.SubjectId!.Value, rf.IndicatorSequence })
            .ToListAsync(cancellationToken);

        var subjectIds = subjectIdsAndSequences.Select(x => x.SubjectId).ToHashSet();

        var dbGroupsLookup = (
            await statisticsDbContext
                .IndicatorGroup.Where(group => subjectIds.Contains(group.SubjectId))
                .Select(group => new { group.SubjectId, group.Id })
                .ToListAsync(cancellationToken)
        ).ToLookup(group => group.SubjectId, group => group.Id);

        var dbIndicatorsLookup = (
            await statisticsDbContext
                .Indicator.Where(indicator => subjectIds.Contains(indicator.IndicatorGroup.SubjectId))
                .Select(indicator => new { indicator.IndicatorGroup.SubjectId, indicator.Id })
                .ToListAsync(cancellationToken)
        ).ToLookup(x => x.SubjectId, x => x.Id);

        foreach (var subjectIdAndSequence in subjectIdsAndSequences)
        {
            var subjectId = subjectIdAndSequence.SubjectId;
            var indicatorSequence = subjectIdAndSequence.IndicatorSequence!;
            var sequenceGroupIds = indicatorSequence.Select(groupEntry => groupEntry.Id).ToHashSet();
            var sequenceIndicatorIds = indicatorSequence.SelectMany(groupEntry => groupEntry.ChildSequence).ToHashSet();

            var dbGroupIds = dbGroupsLookup[subjectId].ToList();
            var dbIndicatorIds = dbIndicatorsLookup[subjectId].ToList();

            var groupsMatch = sequenceGroupIds.SetEquals(dbGroupIds);
            var indicatorsMatch = sequenceIndicatorIds.SetEquals(dbIndicatorIds);

            if (!groupsMatch)
            {
                return BadRequest(
                    $"IndicatorSequence groups incorrect for subject: {subjectId}\nSequenceGroups: {sequenceGroupIds.JoinToString(',')}\nDBGroups: {dbGroupIds.JoinToString(',')}\n"
                );
            }

            if (!indicatorsMatch)
            {
                return BadRequest(
                    $"IndicatorSequence indicators incorrect for subject: {subjectId}\nSequenceIndicators: {sequenceIndicatorIds.JoinToString(',')}\nDBIndicators: {dbIndicatorIds.JoinToString(',')}\n"
                );
            }
        }

        return Ok("All indicator sequences valid");
    }
}
