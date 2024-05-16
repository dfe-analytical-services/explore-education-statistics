using Dapper;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class ParquetService(
    ILogger<ParquetService> logger,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IParquetService
{
    public async Task WriteDataFiles(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var versionDir = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        logger.LogDebug("Writing data files to data set version directory '{VersionDir}'", versionDir);

        await using var duckDb =
            DuckDbConnection.CreateFileConnection(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
        duckDb.Open();

        await duckDb.ExecuteAsync(
            new CommandDefinition(
                $"EXPORT DATABASE '{versionDir}' (FORMAT PARQUET, CODEC ZSTD)",
                cancellationToken
            )
        );

        // Convert absolute paths in load.sql to relative paths otherwise
        // these refer to the machine that the script was run on.

        var loadSqlFilePath = dataSetVersionPathResolver.DuckDbLoadSqlPath(dataSetVersion);

        var absolutePathToReplace = $"{versionDir.Replace('\\', '/')}/";

        var newLines = (await File.ReadAllLinesAsync(loadSqlFilePath, cancellationToken))
            .Select(line => line.Replace(absolutePathToReplace, ""));

        await File.WriteAllLinesAsync(loadSqlFilePath, newLines, cancellationToken);
    }
}
