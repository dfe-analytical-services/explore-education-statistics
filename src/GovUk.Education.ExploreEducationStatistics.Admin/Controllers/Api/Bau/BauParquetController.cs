#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// Temporary endpoints for migrating data sets imported into StatsDB to parquet files. Remove these, along with
/// ExportParquetFunction and ParquetDataSetService.ExportStatsDbDataSet in the data processor and ParquetExportComparer,
/// once every StatsDB data set has been migrated. Exported data files are marked as having parquet files, but continue to be read from StatsDB.
/// </summary>
[Route("api/bau/data-files")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class BauParquetController(
    ContentDbContext contentDbContext,
    IDataProcessorClient dataProcessorClient,
    StatisticsDbDataSetFactory statisticsDbDataSetFactory,
    ParquetV1DataSetFactory parquetV1DataSetFactory
) : ControllerBase
{
    [HttpPost("{fileId:guid}/parquet")]
    public async Task<ActionResult> ExportDataFileToParquet(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await contentDbContext.Files.SingleOrDefaultAsync(
            f => f.Id == fileId && f.Type == FileType.Data,
            cancellationToken
        );

        if (file is null)
        {
            return NotFound();
        }

        if (file.DataStorageVersion != DataStorageVersion.StatsDB)
        {
            return BadRequest($"Data file {fileId} is not stored in StatsDB");
        }

        if (file.HasParquet)
        {
            return BadRequest($"Data file {fileId} already has parquet files");
        }

        var importComplete = await contentDbContext.DataImports.AnyAsync(
            i => i.FileId == fileId && i.Status == DataImportStatus.COMPLETE,
            cancellationToken
        );

        if (!importComplete)
        {
            return BadRequest($"Data file {fileId} has not finished importing");
        }

        await dataProcessorClient.ExportParquet(fileId, cancellationToken);

        return Accepted();
    }

    [HttpPost("parquet")]
    public async Task<ActionResult> ExportDataFilesToParquet(
        int limit = 6,
        bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        // @MarkFix ParquetDataSetService.ExportStatsDbDataSet sets HasParquet without comparing the exported row count
        // with StatsDB. Fail an export whose row counts differ.
        var fileIds = await contentDbContext
            .Files.Where(f =>
                f.Type == FileType.Data
                && f.DataStorageVersion == DataStorageVersion.StatsDB
                && !f.HasParquet
                && contentDbContext.DataImports.Any(i => i.FileId == f.Id && i.Status == DataImportStatus.COMPLETE)
            )
            .OrderBy(f => f.Created)
            .Select(f => f.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (!dryRun)
        {
            foreach (var fileId in fileIds)
            {
                await dataProcessorClient.ExportParquet(fileId, cancellationToken);
            }
        }

        return Ok(
            new
            {
                DryRun = dryRun,
                FileCount = fileIds.Count,
                FileIds = fileIds,
            }
        );
    }

    [HttpGet("{fileId:guid}/parquet/compare")]
    public async Task<ActionResult> CompareDataFileWithParquet(
        Guid fileId,
        CancellationToken cancellationToken = default
    )
    {
        var file = await contentDbContext.Files.SingleOrDefaultAsync(
            f => f.Id == fileId && f.Type == FileType.Data,
            cancellationToken
        );

        if (file is null)
        {
            return NotFound();
        }

        if (file.DataStorageVersion != DataStorageVersion.StatsDB || file.SubjectId is not { } subjectId)
        {
            return BadRequest($"Data file {fileId} is not stored in StatsDB");
        }

        if (!file.HasParquet)
        {
            return BadRequest($"Data file {fileId} has no parquet files");
        }

        var comparison = await ParquetExportComparer.Compare(
            statisticsDbDataSetFactory.Create(subjectId),
            parquetV1DataSetFactory.Create(subjectId),
            cancellationToken
        );

        return Ok(comparison);
    }
}
