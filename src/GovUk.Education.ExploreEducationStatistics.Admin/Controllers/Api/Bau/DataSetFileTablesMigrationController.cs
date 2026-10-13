#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Migrations;
using GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

// TODO EES-5105 Remove once the DataSetFile tables are the source of truth and DataSetFileDualWriteInterceptor is removed
[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class DataSetFileTablesMigrationController(ContentDbContext contentDbContext) : ControllerBase
{
    private const int SampleSize = 20;

    private const string BackfillScriptMigrationId = "20261013151651";

    private const string DataFiles = """
        SELECT DataFile.Id,
               DataFile.DataSetFileId,
               DataFile.DataSetFileVersion AS [Version],
               DataFile.SubjectId,
               DataFile.RootPath,
               DataFile.Filename,
               DataFile.ContentType,
               DataFile.ContentLength,
               MetaFile.Id AS MetaFileId,
               MetaFile.Filename AS MetaFilename,
               MetaFile.ContentType AS MetaContentType,
               MetaFile.ContentLength AS MetaContentLength,
               DataFile.DataSetFileMeta,
               DataFile.FilterHierarchies,
               DataFile.ReplacedById,
               DataFile.ReplacingId,
               DataFile.Created,
               DataFile.CreatedById
        FROM dbo.Files DataFile
                 OUTER APPLY (SELECT TOP 1 Id, Filename, ContentType, ContentLength
                              FROM dbo.Files
                              WHERE Type = 'Metadata'
                                AND SubjectId = DataFile.SubjectId
                              ORDER BY Id) MetaFile
        WHERE DataFile.Type = 'Data'
        """;

    private const string DataSetFileVersions = """
        SELECT Id,
               DataSetFileId,
               [Version],
               SubjectId,
               RootPath,
               Filename,
               ContentType,
               ContentLength,
               MetaFileId,
               MetaFilename,
               MetaContentType,
               MetaContentLength,
               DataSetFileMeta,
               FilterHierarchies,
               ReplacedById,
               ReplacingId,
               Created,
               CreatedById
        FROM dbo.DataSetFileVersions
        """;

    private const string DataReleaseFiles = """
        SELECT ReleaseFile.Id,
               ReleaseFile.ReleaseVersionId,
               ReleaseFile.FileId AS DataSetFileVersionId,
               ReleaseFile.Name,
               ReleaseFile.Summary,
               ReleaseFile.[Order],
               ReleaseFile.PublicApiCompatible,
               ReleaseFile.PublicApiDataSetId,
               ReleaseFile.PublicApiDataSetVersion,
               ReleaseFile.FilterSequence,
               ReleaseFile.IndicatorSequence,
               ReleaseFile.Published
        FROM dbo.ReleaseFiles ReleaseFile
                 JOIN dbo.Files DataFile ON DataFile.Id = ReleaseFile.FileId AND DataFile.Type = 'Data'
        """;

    private const string ReleaseVersionDataSetFileVersions = """
        SELECT Id,
               ReleaseVersionId,
               DataSetFileVersionId,
               Name,
               Summary,
               [Order],
               PublicApiCompatible,
               PublicApiDataSetId,
               PublicApiDataSetVersion,
               FilterSequence,
               IndicatorSequence,
               Published
        FROM dbo.ReleaseVersionDataSetFileVersions
        """;

    public record TableDrift(int Missing, int Extra, int Drifted, List<Guid> Sample);

    public record CheckResult(
        TableDrift DataSetFiles,
        TableDrift DataSetFileVersions,
        TableDrift ReleaseVersionDataSetFileVersions
    );

    public record ResyncResult(
        bool IsDryRun,
        int DataSetFilesInserted,
        int DataSetFilesDeleted,
        int DataSetFileVersionsInserted,
        int DataSetFileVersionsDeleted,
        int ReleaseVersionDataSetFileVersionsInserted,
        int ReleaseVersionDataSetFileVersionsDeleted
    );

    private record BackfillCounts(
        int DataSetFilesInserted,
        int DataSetFilesDeleted,
        int DataSetFileVersionsInserted,
        int DataSetFileVersionsDeleted,
        int ReleaseVersionDataSetFileVersionsInserted,
        int ReleaseVersionDataSetFileVersionsDeleted
    );

    /// <summary>
    /// Reports rows in the DataSetFile tables which are missing, extra or have different values to the Files and
    /// ReleaseFiles rows they mirror.
    /// </summary>
    [HttpGet("data-set-file-tables/check")]
    public async Task<CheckResult> Check(CancellationToken cancellationToken = default)
    {
        // DataSetFiles.Created isn't compared, as it's the earliest Created of its data files at the time it was
        // added, which changes in Files when that data file is deleted.
        var dataSetFiles = await GetDrift(
            source: "SELECT DISTINCT DataSetFileId AS Id FROM dbo.Files WHERE Type = 'Data' AND DataSetFileId IS NOT NULL",
            mirror: "SELECT Id FROM dbo.DataSetFiles",
            cancellationToken
        );

        var dataSetFileVersions = await GetDrift(source: DataFiles, mirror: DataSetFileVersions, cancellationToken);

        var releaseVersionDataSetFileVersions = await GetDrift(
            source: DataReleaseFiles,
            mirror: ReleaseVersionDataSetFileVersions,
            cancellationToken
        );

        return new CheckResult(dataSetFiles, dataSetFileVersions, releaseVersionDataSetFileVersions);
    }

    /// <summary>
    /// Re-runs the backfill of the DataSetFile tables, which inserts missing rows and deletes extra rows.
    /// Rows with different values are left for investigation.
    /// </summary>
    [HttpPut("data-set-file-tables/resync")]
    public async Task<ResyncResult> Resync(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default
    )
    {
        var script = await System.IO.File.ReadAllTextAsync(
            Path.Combine(
                AppContext.BaseDirectory,
                MigrationConstants.ContentMigrationsPath,
                $"{BackfillScriptMigrationId}_{nameof(Ees5105AddDataSetFileTables)}_Backfill.sql"
            ),
            cancellationToken
        );

        contentDbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

        var strategy = contentDbContext.Database.CreateExecutionStrategy();

        var counts = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await contentDbContext.Database.BeginTransactionAsync(cancellationToken);

            var result = (
                await contentDbContext.Database.SqlQueryRaw<BackfillCounts>(script).ToListAsync(cancellationToken)
            ).Single();

            if (dryRun)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            else
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return result;
        });

        return new ResyncResult(
            IsDryRun: dryRun,
            DataSetFilesInserted: counts.DataSetFilesInserted,
            DataSetFilesDeleted: counts.DataSetFilesDeleted,
            DataSetFileVersionsInserted: counts.DataSetFileVersionsInserted,
            DataSetFileVersionsDeleted: counts.DataSetFileVersionsDeleted,
            ReleaseVersionDataSetFileVersionsInserted: counts.ReleaseVersionDataSetFileVersionsInserted,
            ReleaseVersionDataSetFileVersionsDeleted: counts.ReleaseVersionDataSetFileVersionsDeleted
        );
    }

    private async Task<TableDrift> GetDrift(string source, string mirror, CancellationToken cancellationToken)
    {
        var missingIds = await QueryIds(
            $"SELECT Id AS Value FROM ({source}) Source WHERE Id NOT IN (SELECT Id FROM ({mirror}) Mirror)",
            cancellationToken
        );

        var extraIds = await QueryIds(
            $"SELECT Id AS Value FROM ({mirror}) Mirror WHERE Id NOT IN (SELECT Id FROM ({source}) Source)",
            cancellationToken
        );

        // Rows of the source which aren't identical in the mirror, excluding those which are missing entirely
        var differentIds = await QueryIds(
            $"SELECT Id AS Value FROM ({source} EXCEPT {mirror}) Different",
            cancellationToken
        );

        var driftedIds = differentIds.Except(missingIds).ToList();

        return new TableDrift(
            Missing: missingIds.Count,
            Extra: extraIds.Count,
            Drifted: driftedIds.Count,
            Sample: missingIds.Concat(extraIds).Concat(driftedIds).Take(SampleSize).ToList()
        );
    }

    private Task<List<Guid>> QueryIds(string sql, CancellationToken cancellationToken) =>
        contentDbContext.Database.SqlQueryRaw<Guid>(sql).ToListAsync(cancellationToken);
}
