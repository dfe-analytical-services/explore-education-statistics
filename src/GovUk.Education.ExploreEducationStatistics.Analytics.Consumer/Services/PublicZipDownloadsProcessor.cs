using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicZipDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow) : IRequestFileProcessor
{
    private static readonly string[] PublicZipDownloadsSubPath = ["public", "zip-downloads"];
    
    public string SourceDirectory => pathResolver.BuildSourceDirectory(PublicZipDownloadsSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(PublicZipDownloadsSubPath);
    
    public Task Process()
    {
        return workflow.Process(new WorkflowActor(
            sourceDirectory: SourceDirectory,
            reportsDirectory: ReportsDirectory));
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
                CREATE TABLE sourceTable (
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

        public async Task ProcessSourceFiles(string sourceFilesDirectory, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync($@"
                INSERT INTO sourceTable BY NAME (
                    SELECT
                        MD5(CONCAT(subjectId, releaseVersionId, fromPage)) AS zipDownloadHash,
                        *
                    FROM read_json('{sourceFilesDirectory}', 
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
                FROM sourceTable
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
