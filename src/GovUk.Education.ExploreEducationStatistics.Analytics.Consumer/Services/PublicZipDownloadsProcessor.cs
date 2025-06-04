using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicZipDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow) : IRequestFileProcessor
{
    public Task Process()
    {
        return workflow.Process(new WorkflowActor(
            sourceDirectory: pathResolver.PublicZipDownloadsDirectoryPath(),
            reportsDirectory: pathResolver.PublicZipDownloadsReportsDirectoryPath()));
    }

    private class WorkflowActor(string sourceDirectory, string reportsDirectory) 
        : IWorkflowActor
    {
        public string GetSourceDirectory()
        {
            return sourceDirectory;
        }

        public string GetReportsDirectory()
        {
            return reportsDirectory;
        }
        
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
                    dataSetTitle VARCHAR
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
                            dataSetTitle: VARCHAR
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
