#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
        public int LeftToProcess;
    }

    private class TimePeriod
    {
        public int Year;
        public TimeIdentifier TimeIdentifier;
    }

    [HttpPatch("bau/migrate-datasetfilemeta")]
    public async Task<DataSetFileMetaMigrationResult> MigrateReleaseSeries(
        [FromQuery] bool dryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var filesQueryable = _contentDbContext.Files
            .Where(f =>
                f.DataSetFileMeta == null
                && f.Type == FileType.Data);

        if (num is > 0)
        {
            filesQueryable = filesQueryable.Take(num.Value);
        }

        var files = await filesQueryable.ToListAsync(cancellationToken: cancellationToken);

        var numDataSetFileMetaSet = 0;

        foreach (var file in files)
        {
            var observations = _statisticsDbContext.Observation
                .AsNoTracking()
                .Where(o => o.SubjectId == file.SubjectId);

            var geographicLevels = await observations
                .Select(o => o.Location.GeographicLevel)
                .Distinct()
                .OrderBy(gl => gl)
                .ToListAsync(cancellationToken: cancellationToken);

            var timePeriods = await observations
                .Select(o => new TimePeriod{ Year = o.Year, TimeIdentifier = o.TimeIdentifier })
                .Distinct()
                .OrderBy(tp => tp.Year)
                .ToListAsync(cancellationToken: cancellationToken);

            var filters = await _statisticsDbContext.Filter
                .Where(f => f.SubjectId == file.SubjectId)
                .Select(f => new FilterMeta { Id = f.Id, Label = f.Label, })
                .OrderBy(f => f.Label)
                .ToListAsync(cancellationToken: cancellationToken);

            var indicators = await _statisticsDbContext.Indicator
                .Where(i => i.IndicatorGroup.SubjectId == file.SubjectId)
                .Select(i => new IndicatorMeta
                {
                    Id = i.Id,
                    Label = i.Label,
                })
                .OrderBy(i => i.Label)
                .ToListAsync(cancellationToken: cancellationToken);

            file.DataSetFileMeta = new DataSetFileMeta
            {
                GeographicLevels = geographicLevels
                    .Select(gl => gl.GetEnumLabel()).ToList(),
                TimeIdentifier = timePeriods[0].TimeIdentifier,
                Years = timePeriods.Select(tp => tp.Year).ToList(),
                Filters = filters,
                Indicators = indicators,
            };

            numDataSetFileMetaSet++;
        }

        if (!dryRun)
        {
            await _contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new DataSetFileMetaMigrationResult
        {
            IsDryRun = dryRun,
            Processed = numDataSetFileMetaSet,
            LeftToProcess = _contentDbContext.Files
                .Count(f =>
                    f.DataSetFileMeta == null
                    && f.Type == FileType.Data),
        };
    }
}
