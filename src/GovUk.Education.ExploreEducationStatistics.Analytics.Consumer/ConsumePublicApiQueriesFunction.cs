using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public class ConsumePublicApiQueriesFunction(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicApiQueriesFunction> logger)
{
    [Function(nameof(ConsumePublicApiQueriesFunction))]
    public Task Run(
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
            return Task.CompletedTask;
        }
        
        logger.LogInformation("Found {Count} data set queries to process", filesToProcess.Count);

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(sourceDirectory, file);
            var newPath = Path.Combine(processingDirectory, file);
            File.Move(originalPath, newPath);
        });
        
        duckDbConnection.ExecuteNonQuery("install json; load json");

        duckDbConnection.ExecuteNonQuery($@"
            CREATE TABLE queries AS 
            SELECT
                md5(Query) AS QueryHash,
                *
            FROM read_json('{processingDirectory}/*.json', 
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
                }})");

        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE query_report AS 
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
            FROM queries
            GROUP BY QueryHash
            ORDER BY QueryHash");
        
        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE query_access_report AS 
            SELECT 
                QueryHash,
                DataSetVersionId,
                StartTime,
                EndTime,
                extract('milliseconds' FROM EndTime - StartTime) AS DurationMillis
            FROM queries
            ORDER BY QueryHash");

        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        
        var queryReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-api-queries.parquet");
        
        var queryAccessReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-api-query-access.parquet");

        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM query_report)
            TO '{queryReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");
        
        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM query_access_report)
            TO '{queryAccessReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");
        
        Directory.Delete(processingDirectory, recursive: true);
        
        return Task.CompletedTask;
    }
}
