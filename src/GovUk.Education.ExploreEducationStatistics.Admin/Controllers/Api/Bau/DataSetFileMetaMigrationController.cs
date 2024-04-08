#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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

    public class Observation
    {
        public GeographicLevel GeographicLevel;
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
            var observations = await _statisticsDbContext.Observation
                .AsNoTracking()
                .Where(o => o.SubjectId == file.SubjectId)
                .Select(o => new Observation
                {
                    GeographicLevel = o.Location.GeographicLevel,
                    Year = o.Year,
                    TimeIdentifier = o.TimeIdentifier,
                })
                .ToListAsync(cancellationToken: cancellationToken);

            var geographicLevels = observations
                .Select(o => o.GeographicLevel.GetEnumLabel())
                .Distinct()
                .OrderBy(gl => gl)
                .ToList();

            var timePeriods = observations
                .Select(o => (o.Year, o.TimeIdentifier))
                .Distinct()
                .OrderBy(tp => tp.Year)
                .ToList();

            var filters = await _statisticsDbContext.Filter
                .Where(f => f.SubjectId == file.SubjectId)
                .Select(f => new FilterMeta { Id = f.Id, Label = f.Label, })
                .ToListAsync(cancellationToken: cancellationToken);

            var indicators = await _statisticsDbContext.Indicator
                .Where(i => i.IndicatorGroup.SubjectId == file.SubjectId)
                .Select(i => new IndicatorMeta
                {
                    Id = i.Id,
                    Label = i.Label,
                }).ToListAsync(cancellationToken: cancellationToken);

            file.DataSetFileMeta = new DataSetFileMeta
            {
                GeographicLevels = geographicLevels,
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
