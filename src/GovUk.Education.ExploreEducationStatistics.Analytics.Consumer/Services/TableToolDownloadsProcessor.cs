using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class TableToolDownloadsProcessor(IAnalyticsPathResolver pathResolver, IProcessRequestFilesWorkflow workflow)
    : IRequestFileProcessor
{
    public const string ReportFileSuffix = "_public-table-tool-downloads.parquet";

    private static readonly string[] CapturedCallsSubPath = ["public", "table-tool-downloads", "table-tool-page"];

    public string SourceDirectory => pathResolver.BuildSourceDirectory(CapturedCallsSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(CapturedCallsSubPath);

    public async Task Process()
    {
        await workflow.Process(new WorkflowActor(sourceDirectory: SourceDirectory, reportsDirectory: ReportsDirectory));
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
                    tableToolDownloadHash VARCHAR,
                    releaseVersionId UUID,
                    publicationName VARCHAR,
                    releasePeriodAndLabel VARCHAR,
                    subjectId UUID,
                    datasetName VARCHAR,
                    downloadFormat VARCHAR,
                    query JSON
                )
            "
            );
        }

        public async Task ProcessSourceFiles(string sourceFilesDirectory, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(
                $@"
                INSERT INTO sourceTable BY NAME (
                    SELECT
                        MD5(CONCAT(query, releaseVersionId)) AS tableToolDownloadHash,
                        *
                    FROM read_json('{sourceFilesDirectory}', 
                        format='unstructured',
                        columns = {{
                            releaseVersionId: UUID,
                            publicationName: VARCHAR,
                            releasePeriodAndLabel: VARCHAR,
                            subjectId: UUID,
                            dataSetName: VARCHAR,
                            downloadFormat: VARCHAR,
                            query: JSON
                        }}
                    )
                 )
            "
            );
        }

        public async Task CreateParquetReports(string reportsFolderPathAndFilenamePrefix, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(
                @"
                CREATE TABLE tableToolDownloadsReport AS 
                SELECT 
                    tableToolDownloadHash,
                    FIRST(releaseVersionId) AS releaseVersionId,
                    FIRST(publicationName) AS publicationName,
                    FIRST(releasePeriodAndLabel) AS releasePeriodAndLabel,
                    FIRST(subjectId) AS subjectId,
                    FIRST(dataSetName) AS dataSetName,
                    FIRST(downloadFormat) AS downloadFormat,
                    FIRST(query) AS query,
                    CAST(COUNT(tableToolDownloadHash) AS INT) AS downloads
                FROM sourceTable
                GROUP BY tableToolDownloadHash
                ORDER BY tableToolDownloadHash
            "
            );

            var reportFilePath = $"{reportsFolderPathAndFilenamePrefix}{ReportFileSuffix}";

            await connection.ExecuteNonQueryAsync(
                $@"
                COPY (SELECT * FROM tableToolDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')"
            );
        }
    }
}
