using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicZipDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicZipDownloadsProcessor> logger,
    IWorkflowActor<PublicZipDownloadsProcessor>? workflowActor = null) : IRequestFileProcessor
{
    private readonly IWorkflowActor<PublicZipDownloadsProcessor> _workflowActor 
        = workflowActor ?? new WorkflowActor();
    
    public Task Process() {
    
        var workflow = new ProcessRequestFilesWorkflow<PublicZipDownloadsProcessor>(
            sourceDirectory: pathResolver.PublicZipDownloadsDirectoryPath(),
            reportsDirectory: pathResolver.PublicZipDownloadsReportsDirectoryPath(),
            actor: _workflowActor,
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor<PublicZipDownloadsProcessor>
    {
        public async Task InitialiseDuckDb(DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE zipDownloads (
                    zipDownloadHash VARCHAR,
                    publicationName VARCHAR,
                    releaseVersionId UUID,
                    releaseName VARCHAR,
                    releaseLabel VARCHAR,
                    subjectId UUID,
                    dataSetTitle VARCHAR,
                    fromPage VARCHAR
                )
            ");
        }

        public async Task ProcessSourceFile(string sourceFilePath, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync($@"
                INSERT INTO zipDownloads BY NAME (
                    SELECT
                        MD5(CONCAT(subjectId, releaseVersionId)) AS zipDownloadHash,
                        *
                    FROM read_json('{sourceFilePath}', 
                        format='unstructured',
                        columns = {{
                            publicationName: VARCHAR,
                            releaseVersionId: UUID,
                            releaseName: VARCHAR,
                            releaseLabel: VARCHAR,
                            subjectId: UUID,
                            dataSetTitle: VARCHAR,
                            fromPage: VARCHAR
                        }}
                    )
                 )
            ");
        }

        public async Task CreateParquetReports(string reportsFolderPathAndFilenamePrefix, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE zipDownloadsReport AS 
                SELECT 
                    zipDownloadHash,
                    FIRST(publicationName) AS publicationName,
                    FIRST(releaseVersionId) AS releaseVersionId,
                    FIRST(releaseName) AS releaseName,
                    FIRST(releaseLabel) AS releaseLabel,
                    FIRST(subjectId) AS subjectId,
                    FIRST(dataSetTitle) AS dataSetTitle,
                    FIRST(fromPage) AS fromPage,
                    CAST(COUNT(zipDownloadHash) AS INT) AS downloads
                FROM zipDownloads
                GROUP BY zipDownloadHash
                ORDER BY zipDownloadHash
            ");

            var reportFilePath = 
                $"{reportsFolderPathAndFilenamePrefix}_public-zip-downloads.parquet";

            await connection.ExecuteNonQueryAsync($@"
                COPY (SELECT * FROM zipDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
        }
    }
}
