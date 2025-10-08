using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PermalinksTableDownloadsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow
) : IRequestFileProcessor
{
    public const string ReportFileSuffix = "_public-permalink-table-downloads.parquet";

    private static readonly string[] CapturedCallsSubPath = ["public", "table-tool-downloads", "permalinks"];

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
                    permalinkId UUID,
                    permalinkTitle VARCHAR,
                    downloadFormat VARCHAR
                )
            "
            ); // TODO: add Query
        }

        public async Task ProcessSourceFiles(string sourceFilesDirectory, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(
                $@"
                INSERT INTO sourceTable BY NAME (
                    SELECT
                        *
                    FROM read_json('{sourceFilesDirectory}', 
                        format='unstructured',
                        columns = {{
                            permalinkId: UUID,
                            permalinkTitle: VARCHAR,
                            downloadFormat: VARCHAR
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
                CREATE TABLE permalinkTableDownloadsReport AS 
                SELECT 
                    FIRST(permalinkId) AS permalinkId,
                    FIRST(permalinkTitle) AS permalinkTitle,
                    FIRST(downloadFormat) AS downloadFormat,
                    CAST(COUNT(permalinkId) AS INT) AS downloads
                FROM sourceTable
                GROUP BY permalinkId
                ORDER BY permalinkId
            "
            );

            var reportFilePath = $"{reportsFolderPathAndFilenamePrefix}{ReportFileSuffix}";

            await connection.ExecuteNonQueryAsync(
                $@"
                COPY (SELECT * FROM permalinkTableDownloadsReport)
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')"
            );
        }
    }
}
