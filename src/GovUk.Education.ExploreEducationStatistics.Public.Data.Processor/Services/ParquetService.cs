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
    public async Task WriteData(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var versionDir = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        logger.LogInformation("Writing data to data set version directory '{VersionDir}'", versionDir);

        await using var duckDb =
            DuckDbConnection.FileConnection(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
        duckDb.Open();

        await duckDb.ExecuteAsync(
            new CommandDefinition(
                $"EXPORT DATABASE '{versionDir}' (FORMAT PARQUET, CODEC ZSTD)",
                cancellationToken
            )
        );

        // Convert absolute paths in load.sql to relative paths otherwise
        // these refer to the machine that the script was run on.

        var loadSqlFilePath = Path.Combine(versionDir, "load.sql");

        var newLines = (await File.ReadAllLinesAsync(loadSqlFilePath, cancellationToken))
            .Select(line => line.Replace($"{versionDir}{Path.DirectorySeparatorChar}", ""));

        await File.WriteAllLinesAsync(loadSqlFilePath, newLines, cancellationToken);
    }
}
