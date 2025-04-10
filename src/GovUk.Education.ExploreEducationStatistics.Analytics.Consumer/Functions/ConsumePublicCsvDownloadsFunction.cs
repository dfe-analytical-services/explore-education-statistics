using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Functions;

public class ConsumePublicCsvDownloadsFunction(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicCsvDownloadsFunction> logger)
{
    [Function(nameof(ConsumePublicCsvDownloadsFunction))]
    public Task Run(
        [TimerTrigger("%App:ConsumePublicCsvDownloadsCronSchedule%")] // @MarkFix add this whereever it needs to be added
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumePublicCsvDownloadsFunction)} triggered");

        var sourceDirectory = pathResolver.PublicCsvDownloadsDirectoryPath();

        if (!Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("No public csv downloads to process");
            return Task.CompletedTask; 
        }
        
        var filesToProcess = Directory
            .GetFiles(sourceDirectory)
            .Select(Path.GetFileName)
            .Cast<string>()
            .ToList();

        if (filesToProcess.Count == 0)
        {
            logger.LogInformation("No public csv downloads to process");
            return Task.CompletedTask;
        }
        
        logger.LogInformation("Found {Count} csv downloads to process", filesToProcess.Count);

        var processingDirectory = pathResolver.PublicCsvDownloadsProcessingDirectoryPath();
        var reportsDirectory = pathResolver.PublicCsvDownloadsReportsDirectoryPath();
        
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
            CREATE TABLE csvDownloads AS 
            SELECT
                MD5(CONCAT(subjectId, releaseVersionId)) AS csvDownloadHash,
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
            CREATE TABLE csvDownloadsReport AS 
            SELECT 
                csvDownloadHash,
                FIRST(publicationName) AS publicationName,
                FIRST(releaseVersionId) AS releaseVersionId,
                FIRST(releaseName) AS releaseName,
                FIRST(releaseLabel) AS releaseLabel,
                FIRST(subjectId) AS subjectId,
                FIRST(dataSetName) AS dataSetName,
                CAST(COUNT(csvDownloadHash) AS INT) AS downloads
            FROM csvDownloads
            GROUP BY csvDownloadHash
            ORDER BY csvDownloadHash");
        
        var reportFilenamePrefix = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        
        var csvDownloadReportFilename = Path.Combine(
            reportsDirectory, 
            $"{reportFilenamePrefix}_public-csv-downloads.parquet");
        
        duckDbConnection.ExecuteNonQuery($@"
            COPY (SELECT * FROM csvDownloadsReport)
            TO '{csvDownloadReportFilename}' (FORMAT 'parquet', CODEC 'zstd')");
        
        Directory.Delete(processingDirectory, recursive: true);
        
        return Task.CompletedTask;
    }
}
