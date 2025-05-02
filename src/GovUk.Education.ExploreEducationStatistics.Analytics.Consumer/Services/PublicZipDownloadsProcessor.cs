using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicZipDownloadsProcessor(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicZipDownloadsProcessor> logger) : IRequestFileProcessor
{
    public Task Process()
    {
        logger.LogInformation("{PublicZipDownloadsProcessor} triggered", nameof(PublicZipDownloadsProcessor));

        var sourceDirectory = pathResolver.PublicZipDownloadsDirectoryPath();

        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No public zip downloads to process");
            return Task.CompletedTask;
        }

        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No public zip downloads to process");
            return Task.CompletedTask;
        }

        logger.LogInformation("Found {Count} zip downloads to process", filesToProcess.Count);

        var processingDirectory = pathResolver.PublicZipDownloadsProcessingDirectoryPath();
        var reportsDirectory = pathResolver.PublicZipDownloadsReportsDirectoryPath();

        Directory.CreateDirectory(processingDirectory);
        Directory.CreateDirectory(reportsDirectory);

        duckDbConnection.Open();

        duckDbConnection.ExecuteNonQuery("install json; load json");

        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE zipDownloads (
                zipDownloadHash VARCHAR,
                publicationName VARCHAR,
                releaseVersionId UUID,
                releaseName VARCHAR,
                releaseLabel VARCHAR,
                subjectId UUID,
                dataSetTitle VARCHAR
            );
        ");

        {
            var batchNum = 0;
            const int batchSize = 100;

            while (filesToProcess.Count > batchNum * batchSize)
            {
                Directory.CreateDirectory(processingDirectory);

                var filesForThisBatch = filesToProcess
                    .Skip(batchNum++ * batchSize)
                    .Take(batchSize)
                    .ToList();

                Parallel.ForEach(filesForThisBatch, file =>
                {
                    var originalPath = Path.Combine(sourceDirectory, file);
                    var newPath = Path.Combine(processingDirectory, file);
                    File.Move(originalPath, newPath);
                });

                try
                {
                    duckDbConnection.ExecuteNonQuery($@"
                    INSERT INTO zipDownloads BY NAME (
                        SELECT
                            MD5(CONCAT(subjectId, releaseVersionId)) AS zipDownloadHash,
                            *
                        FROM read_json('{processingDirectory}/*', 
                            format='unstructured',
                            columns = {{
                                publicationName: VARCHAR,
                                releaseVersionId: UUID,
                                releaseName: VARCHAR,
                                releaseLabel: VARCHAR,
                                subjectId: UUID,
                                dataSetTitle: VARCHAR
                            }}
                        )
                    )");

                    Directory.Delete(processingDirectory, recursive: true);
                }
                catch (DuckDBException e)
                {
                    logger.LogError(e,
                        "Failed to process a batch of analytics request files. Moving files to failures directory.");
                    MoveBadFilesToFailuresDirectory(filesForThisBatch);
                }
            }
        }

        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE zipDownloadsReport AS 
            SELECT
                zipDownloadHash,
                FIRST(publicationName) AS publicationName,
                FIRST(releaseVersionId) AS releaseVersionId,
                FIRST(releaseName) AS releaseName,
                FIRST(releaseLabel) AS releaseLabel,
                FIRST(subjectId) AS subjectId,
                FIRST(dataSetTitle) AS dataSetTitle,
                CAST(COUNT(zipDownloadHash) AS INT) AS downloads
            FROM zipDownloads
            GROUP BY zipDownloadHash
            ORDER BY zipDownloadHash");

        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var zipDownloadReportFilename = Path.Combine(
            reportsDirectory,
            $"{reportFilenamePrefix}_public-zip-downloads.parquet");

        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM zipDownloadsReport)
            TO '{zipDownloadReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");

        return Task.CompletedTask;
    }

    private void MoveBadFilesToFailuresDirectory(List<string> filenames)
    {
        try
        {
            var failuresDirectoryPath = pathResolver.PublicZipDownloadsFailuresDirectoryPath();
            Directory.CreateDirectory(failuresDirectoryPath);

            Parallel.ForEach(filenames, file =>
            {
                var originalPath = Path.Combine(pathResolver.PublicZipDownloadsProcessingDirectoryPath(), file);
                var newPath = Path.Combine(failuresDirectoryPath, file);
                File.Move(originalPath, newPath);
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to move bad files to failures directory.");
        }
    }
}
