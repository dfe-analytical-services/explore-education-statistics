using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public class ConsumePublicApiQueriesFunction(
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicApiQueriesFunction> logger)
{
    [Function(nameof(ConsumePublicApiQueriesFunction))]
    public async Task Run(
        [TimerTrigger("%App:ConsumePublicApiQueriesCronSchedule%")]
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumePublicApiQueriesFunction)} triggered");

        var sourceDirectory = pathResolver.PublicApiQueriesDirectoryPath();
        var processingDirectory = pathResolver.PublicApiQueriesProcessingDirectoryPath();
        var reportsDirectory = pathResolver.PublicApiQueriesReportsDirectoryPath();
        
        Directory.CreateDirectory(processingDirectory);
        Directory.CreateDirectory(reportsDirectory);
        
        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .Cast<string>()
            .ToList();

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No data set queries to process");
            return;
        }
        
        logger.LogInformation("Found {Count} data set queries to process", filesToProcess.Count);

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(sourceDirectory, file);
            var newPath = Path.Combine(processingDirectory, file);
            File.Move(originalPath, newPath);
        });
        
        await using var duckDbConnection = new DuckDBConnection("DataSource=:memory:");
        duckDbConnection.Open();
        
        await using var command = duckDbConnection.CreateCommand();
        command.CommandText = "install json; load json;";
        command.ExecuteNonQuery();

        command.CommandText = $@"
            CREATE TABLE queries_raw AS 
            SELECT * FROM read_json('{processingDirectory}/*.json', 
                format='auto',
                columns = {{
                    DataSetId: UUID, 
                    DataSetVersionId: UUID,
                    DataSetVersion: VARCHAR,
                    DataSetTitle: VARCHAR,
                    ResultsCount: INT,
                    TotalRowsCount: INT,
                    StartTime: DATETIME,
                    EndTime: DATETIME,
                    Query: JSON
                }});";
        command.ExecuteNonQuery();
        
        command.CommandText = $@"
            CREATE TABLE queries_raw_with_hash AS 
            SELECT md5(Query) AS QueryHash, *
            FROM queries_raw;";
        command.ExecuteNonQuery();

        command.CommandText = @"
            CREATE TABLE queries AS 
            SELECT 
                QueryHash,
                FIRST(DataSetId),
                FIRST(DataSetVersionId),
                FIRST(DataSetVersion),
                FIRST(DataSetTitle),
                FIRST(ResultsCount),
                FIRST(TotalRowsCount),
                FIRST(Query),
                COUNT(QueryHash) AS QueryExecutions
            FROM queries_raw_with_hash
            GROUP BY QueryHash;";
        command.ExecuteNonQuery();
        
        command.CommandText = @"
            CREATE TABLE query_access AS 
            SELECT 
                QueryHash,
                DataSetVersionId,
                StartTime,
                EndTime,
                extract('milliseconds' FROM EndTime - StartTime) AS DurationMillis
            FROM queries_raw_with_hash";
        command.ExecuteNonQuery();

        var reportFilenameSuffix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        
        var queryReportFilename = Path.Combine(
            reportsDirectory, 
            $"public-api-queries_{reportFilenameSuffix}.parquet");
        
        var queryAccessReportFilename = Path.Combine(
            reportsDirectory, 
            $"public-api-query-access_{reportFilenameSuffix}.parquet");

        command.CommandText = $@"
            COPY (SELECT * FROM queries)
            TO '{queryReportFilename}' (FORMAT 'parquet', CODEC 'zstd');";
        command.ExecuteNonQuery();
        
        command.CommandText = $@"
            COPY (SELECT * FROM query_access)
            TO '{queryAccessReportFilename}' (FORMAT 'parquet', CODEC 'zstd');";
        command.ExecuteNonQuery();
    }
}
