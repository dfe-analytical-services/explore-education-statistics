#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class DataSetFileMetaMigrationController : ControllerBase
{
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;

    public DataSetFileMetaMigrationController(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext)
    {
        _contentDbContext = contentDbContext;
        _statisticsDbContext = statisticsDbContext;
    }

    public class DataSetFileMetaMigrationResult
    {
        public bool IsDryRun;
        public int Processed;
        public List<string> Errors;
    }

    private class TimePeriod
    {
        public int Year;
        public TimeIdentifier TimeIdentifier;
    }

    [HttpPatch("bau/migrate-datasetfilemeta-timeperiodrange")]
    public async Task<DataSetFileMetaMigrationResult> MigrateReleaseSeries(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var files = await _contentDbContext.Files
            .Where(f =>
                f.DataSetFileMeta != null
                && f.Type == FileType.Data)
            .ToListAsync();

        var numTimePeriodRangeSet = 0;
        var errors = new List<string>();

        foreach (var file in files)
        {
            var observations = _statisticsDbContext.Observation
                .AsNoTracking()
                .Where(o => o.SubjectId == file.SubjectId);

            var timePeriods = await observations
                .Select(o => new TimePeriodRangeBoundMeta{ Year = o.Year, TimeIdentifier = o.TimeIdentifier })
                .Distinct()
                .OrderBy(tp => tp.Year)
                .ThenBy(tp => tp.TimeIdentifier)
                .ToListAsync(cancellationToken: cancellationToken);

            if (timePeriods.Count == 0)
            {
                var errorStr = $"SubjectId {file.SubjectId} has no observations!";
                errors.Add(errorStr);
                continue;
            }

            _contentDbContext.Update(file);

            file.DataSetFileMeta.TimePeriodRange = new TimePeriodRangeMeta
            {
                Start = timePeriods.First(),
                End = timePeriods.Last(),
            };
            file.DataSetFileMeta.TimeIdentifier = null;
            file.DataSetFileMeta.Years = null;

            numTimePeriodRangeSet++;
        }

        if (!dryRun)
        {
            await _contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new DataSetFileMetaMigrationResult
        {
            IsDryRun = dryRun,
            Processed = numTimePeriodRangeSet,
            Errors = errors,
        };
    }
}
