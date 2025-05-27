using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicCsvDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicCsvDownloadsProcessor> logger,
    IWorkflowActor<PublicCsvDownloadsProcessor>? workflowActor = null) : IRequestFileProcessor
{
    private readonly IWorkflowActor<PublicCsvDownloadsProcessor> _workflowActor
        = workflowActor ?? new WorkflowActor();

    public Task Process() {
    
        var workflow = new ProcessRequestFilesWorkflow<PublicCsvDownloadsProcessor>(
            sourceDirectory: pathResolver.PublicCsvDownloadsDirectoryPath(),
            reportsDirectory: pathResolver.PublicCsvDownloadsReportsDirectoryPath(),
            actor: _workflowActor,
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor<PublicCsvDownloadsProcessor>
    {
        public async Task InitialiseDuckDb(DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE csvDownloads (
                    csvDownloadHash VARCHAR,
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
                INSERT INTO csvDownloads BY NAME (
                    SELECT
                        MD5(CONCAT(subjectId, releaseVersionId)) AS csvDownloadHash,
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
                CREATE TABLE csvDownloadsReport AS 
                SELECT 
                    csvDownloadHash,
                    FIRST(publicationName) AS publicationName,
                    FIRST(releaseVersionId) AS releaseVersionId,
                    FIRST(releaseName) AS releaseName,
                    FIRST(releaseLabel) AS releaseLabel,
                    FIRST(subjectId) AS subjectId,
                    FIRST(dataSetTitle) AS dataSetTitle,
                    CAST(COUNT(csvDownloadHash) AS INT) AS downloads
                FROM csvDownloads
                GROUP BY csvDownloadHash
                ORDER BY csvDownloadHash
            ");
        
            var reportFilePath = 
                $"{reportsFolderPathAndFilenamePrefix}_public-csv-downloads.parquet";

            await connection.ExecuteNonQueryAsync($@"
                COPY (SELECT * FROM csvDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
        }
    }
}
