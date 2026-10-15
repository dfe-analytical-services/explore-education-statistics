#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

/// <summary>
/// Queues exports of data sets imported into StatsDB to parquet files. Exported data files are marked as having
/// parquet files, but continue to be read from StatsDB.
/// </summary>
[Route("api/bau/data-files")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class BauParquetController(ContentDbContext contentDbContext, IDataProcessorClient dataProcessorClient)
    : ControllerBase
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
        int? limit = null,
        bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        var query = contentDbContext
            .Files.Where(f =>
                f.Type == FileType.Data
                && f.DataStorageVersion == DataStorageVersion.StatsDB
                && !f.HasParquet
                && contentDbContext.DataImports.Any(i => i.FileId == f.Id && i.Status == DataImportStatus.COMPLETE)
            )
            .OrderBy(f => f.Created)
            .Select(f => f.Id);

        if (limit is not null)
        {
            query = query.Take(limit.Value);
        }

        var fileIds = await query.ToListAsync(cancellationToken);

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
}
