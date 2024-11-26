using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;

[Route("api")]
[ApiController]
[Authorize(Roles = GlobalRoles.RoleNames.BauUser)]
public class DataSetFileMetaMigrationController(ContentDbContext contentDbContext) : ControllerBase
{
    public class DataSetFileMetaMigrationResult
    {
        public bool IsDryRun;
        public int Processed;
    }

    [HttpPut("bau/migrate-datasetfilemeta")]
    public async Task<DataSetFileMetaMigrationResult> MigrationDataSetFileMeta(
        [FromQuery] bool dryRun = true,
        [FromQuery] int? num = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = contentDbContext.Files
            .Where(f => f.DataSetFileMeta == null
                && f.DataSetFileMetaOld != null);

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var files = queryable.ToList();

        var numProcessed = 0;

        foreach (var file in files)
        {
            var oldMeta = file.DataSetFileMetaOld;
            file.DataSetFileMeta = new DataSetFileMeta
            {
                GeographicLevels = oldMeta!.GeographicLevels
                    .Select(gl => new GeographicLevelMeta { Code = gl, })
                    .ToList(),
                TimePeriodRange = new TimePeriodRangeMeta
                {
                    Start = new TimePeriodRangeBoundMeta
                    {
                        Period = oldMeta.TimePeriodRange.Start.Period,
                        TimeIdentifier = oldMeta.TimePeriodRange.Start.TimeIdentifier,
                    },
                    End = new TimePeriodRangeBoundMeta
                    {
                        Period = oldMeta.TimePeriodRange.End.Period,
                        TimeIdentifier = oldMeta.TimePeriodRange.End.TimeIdentifier,
                    },
                },
                Filters = oldMeta.Filters
                    .Select(filter => new FilterMeta
                    {
                        FilterId = filter.Id,
                        Label = filter.Label,
                        ColumnName = filter.ColumnName,
                        Hint = filter.Hint,
                    }).ToList(),
                Indicators = oldMeta.Indicators
                    .Select(i => new IndicatorMeta
                    {
                        IndicatorId = i.Id,
                        Label = i.Label,
                        ColumnName = i.ColumnName,
                    }).ToList(),
            };
            numProcessed++;
        }

        if (!dryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        return new DataSetFileMetaMigrationResult
        {
            IsDryRun = dryRun,
            Processed = numProcessed,
        };
    }

}
