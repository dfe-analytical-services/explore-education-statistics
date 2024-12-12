using System;
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
        var migratedDataSetFileIds = contentDbContext.DataSetFileMetaGeographicLevels
            .Select(meta => meta.DataSetFileId)
            .Distinct()
            .ToList();

        migratedDataSetFileIds.Concat(contentDbContext.DataSetFileMetaTimePeriods
            .Select(meta => meta.DataSetFileId)
            .Distinct()
            .ToList());

        migratedDataSetFileIds.Concat(contentDbContext.DataSetFileMetaFilters
            .Select(meta => meta.DataSetFileId)
            .Distinct()
            .ToList());

        migratedDataSetFileIds.Concat(contentDbContext.DataSetFileMetaIndicators
            .Select(meta => meta.DataSetFileId)
            .Distinct()
            .ToList());

        var queryable = contentDbContext.Files
            .Where(f => f.DataSetFileId != null && migratedDataSetFileIds.Contains(f.DataSetFileId.Value));

        if (num != null)
        {
            queryable = queryable.Take(num.Value);
        }

        var files = queryable.ToList();

        var numProcessed = 0;

        foreach (var file in files)
        {
            var oldMeta = file.DataSetFileMeta ?? throw new Exception(
                $"DataSetFileMeta shouldn't be null for file {file.Id}");

            await contentDbContext.DataSetFileMetaGeographicLevels.AddRangeAsync(
                oldMeta.GeographicLevels.Select(gl => new DataSetFileMetaGeographicLevel
                {
                    DataSetFileId = file.DataSetFileId!.Value,
                    GeographicLevel = gl,
                }).ToList(), cancellationToken);

            await contentDbContext.DataSetFileMetaTimePeriods.AddAsync(
                new DataSetFileMetaTimePeriod
                {
                    StartTimeIdentifier = oldMeta.TimePeriodRange.Start.TimeIdentifier,
                    StartPeriod = oldMeta.TimePeriodRange.Start.Period,
                    EndTimeIdentifier = oldMeta.TimePeriodRange.End.TimeIdentifier,
                    EndPeriod = oldMeta.TimePeriodRange.End.Period,
                }, cancellationToken);

            await contentDbContext.DataSetFileMetaFilters.AddRangeAsync(
                oldMeta.Filters.Select(filter => new DataSetFileMetaFilter
                {
                    DataSetFileId = file.Id,
                    FilterId = filter.Id,
                    Label = filter.Label,
                    Hint = filter.Hint,
                    ColumnName = filter.ColumnName,
                }).ToList());

            await contentDbContext.DataSetFileMetaIndicators.AddRangeAsync(
                oldMeta.Indicators.Select(indicator => new DataSetFileMetaIndicator
                {
                    DataSetFileId = file.Id,
                    IndicatorId = indicator.Id,
                    Label = indicator.Label,
                    ColumnName = indicator.ColumnName,
                }).ToList());

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
