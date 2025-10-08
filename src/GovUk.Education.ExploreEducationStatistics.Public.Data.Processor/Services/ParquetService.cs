using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class ParquetService(
    ILogger<ParquetService> logger,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IParquetService
{
    public async Task WriteDataFiles(Guid dataSetVersionId, CancellationToken cancellationToken = default)
    {
        var dataSetVersion = await publicDataDbContext.DataSetVersions.SingleAsync(
            dsv => dsv.Id == dataSetVersionId,
            cancellationToken: cancellationToken
        );

        var versionDir = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        logger.LogDebug("Writing data files to data set version directory '{VersionDir}'", versionDir);

        await using var duckDbConnection = DuckDbConnection.CreateFileConnectionReadOnly(
            dataSetVersionPathResolver.DuckDbPath(dataSetVersion)
        );

        await duckDbConnection
            .SqlBuilder($"EXPORT DATABASE '{versionDir:raw}' (FORMAT PARQUET, CODEC ZSTD)")
            .ExecuteAsync(cancellationToken: cancellationToken);

        // Convert absolute paths in load.sql to relative paths otherwise
        // these refer to the machine that the script was run on.

        var loadSqlFilePath = dataSetVersionPathResolver.DuckDbLoadSqlPath(dataSetVersion);

        var absolutePathToReplace = $"{versionDir.Replace('\\', '/')}/";

        var newLines = (await File.ReadAllLinesAsync(loadSqlFilePath, cancellationToken)).Select(line =>
            line.Replace(absolutePathToReplace, "")
        );

        await File.WriteAllLinesAsync(loadSqlFilePath, newLines, cancellationToken);
    }
}
