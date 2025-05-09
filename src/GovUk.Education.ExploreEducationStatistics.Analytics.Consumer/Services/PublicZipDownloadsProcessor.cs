using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
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
            actor: new WorkflowActor(),
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor
    {
        public void InitialiseDuckDb(DuckDbConnection connection)
        {
            connection.ExecuteNonQuery(@"
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

        public void ProcessSourceFile(string sourceFilePath, DuckDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
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
                            dataSetTitle: VARCHAR
                        }}
                    )
                 )
            ");
        }

        public void CreateParquetReports(string reportsFilePathAndFilenamePrefix, DuckDbConnection connection)
        {
            connection.ExecuteNonQuery(@"
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
                ORDER BY zipDownloadHash
            ");
        
            var reportFilePath = 
                $"{reportsFilePathAndFilenamePrefix}_public-zip-downloads.parquet";

            connection.ExecuteNonQuery($@"
                COPY (SELECT * FROM zipDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
        }
    }
}
