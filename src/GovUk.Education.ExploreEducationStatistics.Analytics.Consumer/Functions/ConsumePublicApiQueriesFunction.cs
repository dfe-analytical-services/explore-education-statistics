using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Functions;

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

        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No data set queries to process");
            return Task.CompletedTask; 
        }
        
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

        var processingDirectory = pathResolver.PublicApiQueriesProcessingDirectoryPath();
        var reportsDirectory = pathResolver.PublicApiQueriesReportsDirectoryPath();
        
        Directory.CreateDirectory(processingDirectory);
        Directory.CreateDirectory(reportsDirectory);

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(sourceDirectory, file);
            var newPath = Path.Combine(processingDirectory, file);
            File.Move(originalPath, newPath);
        });
        
        duckDbConnection.Open();
        
        duckDbConnection.ExecuteNonQuery("install json; load json");

        duckDbConnection.ExecuteNonQuery($@"
            CREATE TABLE Queries AS 
            SELECT
                MD5(CONCAT(Query, DataSetVersionId)) AS QueryVersionHash,
                MD5(Query) AS QueryHash,
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
            CREATE TABLE QueryReport AS 
            SELECT 
                QueryVersionHash,
                FIRST(QueryHash) AS QueryHash,
                FIRST(DataSetId) AS DataSetId,
                FIRST(DataSetVersionId) AS DataSetVersionId,
                FIRST(DataSetVersion) AS DataSetVersion,
                FIRST(DataSetTitle) AS DataSetTitle,
                FIRST(ResultsCount) AS ResultsCount,
                FIRST(TotalRowsCount) AS TotalRowsCount,
                FIRST(Query) AS Query,
                CAST(COUNT(QueryHash) AS INT) AS QueryExecutions
            FROM Queries
            GROUP BY QueryVersionHash
            ORDER BY QueryVersionHash");
        
        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE QueryAccessReport AS 
            SELECT 
                QueryVersionHash,
                DataSetVersionId,
                StartTime,
                EndTime,
                CAST(EXTRACT('milliseconds' FROM EndTime - StartTime) AS INT) AS DurationMillis
            FROM Queries
            ORDER BY QueryHash, StartTime");

        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        
        var queryReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-api-queries.parquet");
        
        var queryAccessReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-api-query-access.parquet");

        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM QueryReport)
            TO '{queryReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");
        
        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM QueryAccessReport)
            TO '{queryAccessReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");
        
        Directory.Delete(processingDirectory, recursive: true);
        
        return Task.CompletedTask;
    }
}
