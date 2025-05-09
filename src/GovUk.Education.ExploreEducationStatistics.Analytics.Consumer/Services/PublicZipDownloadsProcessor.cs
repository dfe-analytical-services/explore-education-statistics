using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicZipDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicZipDownloadsProcessor> logger) : IRequestFileProcessor
{
    public Task Process() {
    
        var workflow = new ProcessRequestFilesWorkflow(
            processorName: GetType().Name,
            sourceDirectory: pathResolver.PublicZipDownloadsDirectoryPath(),
            reportsDirectory: pathResolver.PublicZipDownloadsReportsDirectoryPath(),
            initialiseAction: InitialiseDuckDb,
            processSourceFileAction: ProcessSourceFile,
            createParquetReportsAction: CreateParquetReports,
            logger: logger);

        return workflow.Process();
    }

    private static void InitialiseDuckDb(InitialiseDuckDbContext context)
    {
        context.Connection.ExecuteNonQuery(@"
            CREATE TABLE zipDownloads (
                zipDownloadHash VARCHAR,
                publicationName VARCHAR,
                releaseVersionId UUID,
                releaseName VARCHAR,
                releaseLabel VARCHAR,
                subjectId UUID,
                dataSetTitle VARCHAR
            )
        ");
    }

    private static void ProcessSourceFile(ProcessSourceFileContext context)
    {
        context.Connection.ExecuteNonQuery($@"
            INSERT INTO zipDownloads BY NAME (
                SELECT
                    MD5(CONCAT(subjectId, releaseVersionId)) AS zipDownloadHash,
                    *
                FROM read_json('{context.SourceFilePath}', 
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

    private static void CreateParquetReports(CreateParquetReportsContext context)
    {
        context.Connection.ExecuteNonQuery(@"
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
        
        var reportFilePath = 
            $"{context.ReportsFilePathAndFilenamePrefix}_public-zip-downloads.parquet";

        context.Connection.ExecuteNonQuery($@"
            COPY (SELECT * FROM zipDownloadsReport)
            TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
    }
}
