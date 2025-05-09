using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiQueriesProcessor(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicApiQueriesProcessor> logger) : IRequestFileProcessor
{
    public Task Process()
    {
        logger.LogInformation("{PublicApiQueriesProcessor} triggered", nameof(PublicApiQueriesProcessor));

        var sourceDirectory = pathResolver.PublicApiQueriesDirectoryPath();

        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No data set queries to process");
            return Task.CompletedTask;
        }
        
        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
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

        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE queries (
                queryVersionHash VARCHAR,
                queryHash VARCHAR,
                dataSetId UUID,
                dataSetVersionId UUID,
                dataSetVersion VARCHAR,
                dataSetTitle VARCHAR,
                resultsCount INT,
                totalRowsCount INT,
                startTime DATETIME,
                endTime DATETIME,
                query JSON
            );
        ");

        // We fetch the files again in case there are files leftover in the processing dir from a previous function run
        var filesReadyForProcessing = Directory
            .GetFiles(processingDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        foreach (var filename in filesReadyForProcessing)
        {
            try
            {
                duckDbConnection.ExecuteNonQuery($@"
                    INSERT INTO queries BY NAME (
                        SELECT
                            MD5(CONCAT(query, dataSetVersionId)) AS queryVersionHash,
                            MD5(query) AS queryHash,
                            *
                        FROM read_json('{processingDirectory}/{filename}', 
                            format='unstructured',
                            columns = {{
                                dataSetId: UUID, 
                                dataSetVersionId: UUID,
                                dataSetVersion: VARCHAR,
                                dataSetTitle: VARCHAR,
                                resultsCount: INT,
                                totalRowsCount: INT,
                                startTime: DATETIME,
                                endTime: DATETIME,
                                query: JSON
                            }})
                       )
                ");
            }
            catch (DuckDBException e)
            {
                logger.LogError(e, "Failed to process analytics request file {Filename}", filename);
                var badFilePath = Path.Combine(processingDirectory, filename);
                MoveBadFileToFailuresDirectory(badFilePath);
            }
        }

        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE queryReport AS 
            SELECT
                queryVersionHash ,
                FIRST(queryHash) AS queryHash,
                FIRST(dataSetId) AS dataSetId,
                FIRST(dataSetVersionId) AS dataSetVersionId,
                FIRST(dataSetVersion) AS dataSetVersion,
                FIRST(dataSetTitle) AS dataSetTitle,
                FIRST(resultsCount) AS resultsCount,
                FIRST(totalRowsCount) AS totalRowsCount,
                FIRST(query) AS query,
                CAST(COUNT(queryHash) AS INT) AS queryExecutions
            FROM queries
            GROUP BY queryVersionHash
            ORDER BY queryVersionHash");
        
        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE queryAccessReport AS 
            SELECT 
                queryVersionHash,
                dataSetVersionId,
                startTime,
                endTime,
                CAST(EXTRACT('milliseconds' FROM EndTime - StartTime) AS INT) AS durationMillis
            FROM queries
            ORDER BY queryHash, startTime");

        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var queryReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-api-queries.parquet");
        
        var queryAccessReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-api-query-access.parquet");

        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM queryReport)
            TO '{queryReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");

        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM queryAccessReport)
            TO '{queryAccessReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");

        Directory.Delete(processingDirectory, recursive: true);
        
        return Task.CompletedTask;
    }

    private void MoveBadFileToFailuresDirectory(string filePath)
    {
        var failuresDirectoryPath = pathResolver.PublicApiQueriesFailuresDirectoryPath();

        try
        {
            Directory.CreateDirectory(failuresDirectoryPath);
            File.Move(filePath, failuresDirectoryPath);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to move bad file to failures directory. SourcePath: {FilePath}. DestPath: {FailuresDirectoryPath}",
                filePath,
                failuresDirectoryPath);
        }
    }
}
