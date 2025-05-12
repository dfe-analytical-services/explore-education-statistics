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

        Parallel.ForEach(filesToProcess, file =>
        {
            var originalPath = Path.Combine(sourceDirectory, file);
            var newPath = Path.Combine(processingDirectory, file);
            File.Move(originalPath, newPath);
        });

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
                    INSERT INTO zipDownloads BY NAME (
                        SELECT
                            MD5(CONCAT(subjectId, releaseVersionId)) AS zipDownloadHash,
                            *
                        FROM read_json('{processingDirectory}/{filename}', 
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
                     )
                ");
            }
            catch (DuckDBException e)
            {
                logger.LogError(e, "Failed to process zip download request file {Filename}", filename);
                MoveBadFileToFailuresDirectory(filename);
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

        Directory.Delete(processingDirectory, recursive: true);

        return Task.CompletedTask;
    }

    private void MoveBadFileToFailuresDirectory(string filename)
    {
        try
        {
            var failuresDirectoryPath = pathResolver.PublicZipDownloadsFailuresDirectoryPath();
            Directory.CreateDirectory(failuresDirectoryPath);

            var fileSourcePath = Path.Combine(pathResolver.PublicZipDownloadsProcessingDirectoryPath(), filename);
            var fileDestPath = Path.Combine(failuresDirectoryPath, filename);
            File.Move(fileSourcePath, fileDestPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to move bad file to failures directory {Filename}", filename);
        }
    }
}
