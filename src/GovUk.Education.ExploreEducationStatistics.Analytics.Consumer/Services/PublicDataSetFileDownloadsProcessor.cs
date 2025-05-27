using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicDataSetFileDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicDataSetFileDownloadsProcessor> logger,
    IWorkflowActor<PublicDataSetFileDownloadsProcessor>? workflowActor = null) : IRequestFileProcessor
{
    private readonly IWorkflowActor<PublicDataSetFileDownloadsProcessor> _workflowActor
        = workflowActor ?? new WorkflowActor();

    public Task Process() {
    
        var workflow = new ProcessRequestFilesWorkflow<PublicDataSetFileDownloadsProcessor>(
            sourceDirectory: pathResolver.PublicDataSetFileDownloadsDirectoryPath(),
            reportsDirectory: pathResolver.PublicDataSetFileDownloadsReportsDirectoryPath(),
            actor: _workflowActor,
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor<PublicDataSetFileDownloadsProcessor>
    {
        public async Task InitialiseDuckDb(DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE dataSetFileDownloads (
                    dataSetFileDownloadHash VARCHAR,
                    publicationName VARCHAR,
                    releaseVersionId UUID,
                    releaseName VARCHAR,
                    releaseLabel VARCHAR,
                    subjectId UUID,
                    dataSetTitle VARCHAR
                )
            ");
        }

        public async Task ProcessSourceFile(string sourceFilePath, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync($@"
                INSERT INTO dataSetFileDownloads BY NAME (
                    SELECT
                        MD5(CONCAT(subjectId, releaseVersionId)) AS dataSetFileDownloadHash,
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

        public async Task CreateParquetReports(string reportsFolderPathAndFilenamePrefix, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE dataSetFileDownloadsReport AS 
                SELECT 
                    dataSetFileDownloadHash,
                    FIRST(publicationName) AS publicationName,
                    FIRST(releaseVersionId) AS releaseVersionId,
                    FIRST(releaseName) AS releaseName,
                    FIRST(releaseLabel) AS releaseLabel,
                    FIRST(subjectId) AS subjectId,
                    FIRST(dataSetTitle) AS dataSetTitle,
                    CAST(COUNT(dataSetFileDownloadHash) AS INT) AS downloads
                FROM dataSetFileDownloads
                GROUP BY dataSetFileDownloadHash
                ORDER BY dataSetFileDownloadHash
            ");
        
            var reportFilePath = 
                $"{reportsFolderPathAndFilenamePrefix}_public-csv-downloads.parquet";

            await connection.ExecuteNonQueryAsync($@"
                COPY (SELECT * FROM dataSetFileDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
        }
    }
}
