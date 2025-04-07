using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Functions;

public class ConsumePublicZipDownloadsFunction(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicZipDownloadsFunction> logger)
{
    [Function(nameof(ConsumePublicZipDownloadsFunction))]
    public Task Run(
        [TimerTrigger("%App:ConsumePublicZipDownloadsCronSchedule%")]
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumePublicZipDownloadsFunction)} triggered");

        var sourceDirectory = pathResolver.PublicZipDownloadsDirectoryPath();

        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No public zip downloads to process");
            return Task.CompletedTask; 
        }
        
        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .Cast<string>()
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

        duckDbConnection.ExecuteNonQuery($@"
            CREATE TABLE zipDownloads AS 
            SELECT
                MD5(CONCAT(subjectId, releaseVersionId)) AS zipDownloadHash,
                *
            FROM read_json('{processingDirectory}/*.json', 
                format='auto',
                columns = {{
                    publicationName: VARCHAR,
                    releaseVersionId: UUID,
                    releaseName: VARCHAR,
                    releaseLabel: VARCHAR,
                    subjectId: UUID,
                    dataSetName: VARCHAR
                }})");

        duckDbConnection.ExecuteNonQuery(@"
            CREATE TABLE zipDownloadsReport AS 
            SELECT 
                zipDownloadHash,
                FIRST(publicationName) AS publicationName,
                FIRST(releaseVersionId) AS releaseVersionId,
                FIRST(releaseName) AS releaseName,
                FIRST(releaseLabel) AS releaseLabel,
                FIRST(subjectId) AS subjectId,
                FIRST(dataSetName) AS dataSetName,
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
}
