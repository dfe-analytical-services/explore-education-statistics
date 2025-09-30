using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicCsvDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow
) : IRequestFileProcessor
{
    private static readonly string[] PublicCsvDownloadsSubPath = ["public", "csv-downloads"];

    public string SourceDirectory => pathResolver.BuildSourceDirectory(PublicCsvDownloadsSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(PublicCsvDownloadsSubPath);

    public Task Process()
    {
        return workflow.Process(
            new WorkflowActor(sourceDirectory: SourceDirectory, reportsDirectory: ReportsDirectory)
        );
    }

    private class WorkflowActor(string sourceDirectory, string reportsDirectory) : IWorkflowActor
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
            await connection.ExecuteNonQueryAsync(
                @"
                CREATE TABLE sourceTable (
                    csvDownloadHash VARCHAR,
                    publicationName VARCHAR,
                    releaseVersionId UUID,
                    releaseName VARCHAR,
                    releaseLabel VARCHAR,
                    subjectId UUID,
                    dataSetTitle VARCHAR
                )
            "
            );
        }

        public async Task ProcessSourceFiles(
            string sourceFilesDirectory,
            DuckDbConnection connection
        )
        {
            await connection.ExecuteNonQueryAsync(
                $@"
                INSERT INTO sourceTable BY NAME (
                    SELECT
                        MD5(CONCAT(subjectId, releaseVersionId)) AS csvDownloadHash,
                        *
                    FROM read_json('{sourceFilesDirectory}', 
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
            "
            );
        }

        public async Task CreateParquetReports(
            string reportsFolderPathAndFilenamePrefix,
            DuckDbConnection connection
        )
        {
            await connection.ExecuteNonQueryAsync(
                @"
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
                FROM sourceTable
                GROUP BY csvDownloadHash
                ORDER BY csvDownloadHash
            "
            );

            var reportFilePath =
                $"{reportsFolderPathAndFilenamePrefix}_public-csv-downloads.parquet";

            await connection.ExecuteNonQueryAsync(
                $@"
                COPY (SELECT * FROM csvDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')"
            );
        }
    }
}
