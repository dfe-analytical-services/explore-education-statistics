using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiDataSetVersionCallsProcessor(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicApiDataSetVersionCallsProcessor> logger) : IRequestFileProcessor
{
    public Task Process()
    {
        logger.LogInformation("{Processor} triggered", nameof(PublicApiDataSetVersionCallsProcessor));

        var sourceDirectory = pathResolver.PublicApiDataSetVersionCallsDirectoryPath();

        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No Public API DataSetVersion requests to process");
            return Task.CompletedTask;
        }

        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No Public API DataSetVersion requests to process");
            return Task.CompletedTask;
        }

        logger.LogInformation("Found {Count} requests to process", filesToProcess.Count);

        var processingDirectory = pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath();
        var reportsDirectory = pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath();

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
            CREATE TABLE DataSetVersionCalls (
                dataSetId UUID,
                dataSetTitle VARCHAR,
                dataSetVersion VARCHAR,
                dataSetVersionId UUID,
                parameters JSON,
                previewToken JSON,
                requestedDataSetVersion VARCHAR,
                startTime DATETIME,
                type VARCHAR
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
                    INSERT INTO DataSetVersionCalls BY NAME (
                        SELECT *
                        FROM read_json('{processingDirectory}/{filename}', 
                            format='unstructured'
                        )
                     )
                ");
            }
            catch (DuckDBException e)
            {
                logger.LogError(e, "Failed to process analytics request file {Filename}", filename);
                MoveBadFileToFailuresDirectory(filename);
            }
        }

        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var reportFilename = Path.Combine(
            reportsDirectory,
            $"{reportFilenamePrefix}_public-api-data-set-version-calls.parquet");

        duckDbConnection.ExecuteNonQuery($@"
            COPY (
                SELECT * EXCLUDE previewToken,
                previewToken->>'label' AS previewTokenLabel,
                CAST(previewToken->>'dataSetVersionId' AS UUID) AS previewTokenDataSetVersionId,
                CAST(previewToken->>'created' AS DATETIME) AS previewTokenCreated,
                CAST(previewToken->>'expiry' AS DATETIME) AS previewTokenExpiry 
                FROM DataSetVersionCalls 
                ORDER BY startTime ASC
            )
            TO '{reportFilename}' (FORMAT 'parquet', CODEC 'zstd')");

        Directory.Delete(processingDirectory, recursive: true);

        return Task.CompletedTask;
    }

    private void MoveBadFileToFailuresDirectory(string filename)
    {
        try
        {
            var failuresDirectoryPath = pathResolver.PublicApiDataSetVersionCallsFailuresDirectoryPath();
            Directory.CreateDirectory(failuresDirectoryPath);

            var fileSourcePath = Path.Combine(pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath(), filename);
            var fileDestPath = Path.Combine(failuresDirectoryPath, filename);
            File.Move(fileSourcePath, fileDestPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to move bad file to failures directory {Filename}", filename);
        }
    }
}
