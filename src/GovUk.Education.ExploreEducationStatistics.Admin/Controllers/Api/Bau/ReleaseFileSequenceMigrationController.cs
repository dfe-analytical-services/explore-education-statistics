#nullable enable
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class ReleaseFileSequenceMigrationController(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext)
    : ControllerBase
{
    public class ReleaseFileSequenceMigrationResult
    {
        public bool IsDryRun;
        public int NumReleaseSubjectsMigrated;
    }

    [HttpPatch("bau/migrate-release-file")]
    public async Task<ReleaseFileSequenceMigrationResult> MigrateReleaseFileSequence(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var releaseSubjects = await statisticsDbContext.ReleaseSubject
            .Where(rs => rs.FilterSequence != null || rs.IndicatorSequence != null)
            .ToListAsync(cancellationToken: cancellationToken);

        var numReleaseSubjectsMigrated = 0;

        foreach (var releaseSubject in releaseSubjects)
        {
            var releaseFile = await contentDbContext.ReleaseFiles
                .Where(rf =>
                    rf.File.Type == FileType.Data
                    && rf.File.SubjectId == releaseSubject.SubjectId
                    && rf.ReleaseVersionId == releaseSubject.ReleaseVersionId)
                .SingleAsync(cancellationToken: cancellationToken);

            releaseFile.FilterSequence = releaseSubject.FilterSequence;
            releaseFile.IndicatorSequence = releaseSubject.IndicatorSequence;

            numReleaseSubjectsMigrated++;
        }

        if (!dryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new ReleaseFileSequenceMigrationResult
        {
            IsDryRun = dryRun,
            NumReleaseSubjectsMigrated = numReleaseSubjectsMigrated,
        };
    }
}
