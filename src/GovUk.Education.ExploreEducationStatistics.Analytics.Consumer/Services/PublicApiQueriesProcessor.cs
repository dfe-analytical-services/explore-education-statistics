using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiQueriesProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow
) : IRequestFileProcessor
{
    private static readonly string[] PublicApiQueriesSubPath = ["public-api", "queries"];

    public string SourceDirectory => pathResolver.BuildSourceDirectory(PublicApiQueriesSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(PublicApiQueriesSubPath);

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
                    queryVersionHash VARCHAR,
                    queryHash VARCHAR,
                    dataSetId UUID,
                    dataSetVersionId UUID,
                    dataSetVersion VARCHAR,
                    dataSetTitle VARCHAR,
                    previewToken JSON,
                    requestedDataSetVersion VARCHAR,
                    resultsCount INT,
                    totalRowsCount INT,
                    startTime DATETIME,
                    endTime DATETIME,
                    query JSON
                );
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
                        MD5(CONCAT(query, dataSetVersionId)) AS queryVersionHash,
                        MD5(query) AS queryHash,
                        *
                    FROM read_json('{sourceFilesDirectory}', 
                        format='unstructured',
                        columns = {{
                            dataSetId: UUID, 
                            dataSetVersionId: UUID,
                            dataSetVersion: VARCHAR,
                            dataSetTitle: VARCHAR,
                            previewToken: JSON,
                            requestedDataSetVersion: VARCHAR,
                            resultsCount: INT,
                            totalRowsCount: INT,
                            startTime: DATETIME,
                            endTime: DATETIME,
                            query: JSON
                        }})
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
                CREATE TABLE queryReport AS 
                SELECT
                    queryVersionHash ,
                    FIRST(queryHash) AS queryHash,
                    FIRST(dataSetId) AS dataSetId,
                    FIRST(dataSetVersionId) AS dataSetVersionId,
                    FIRST(dataSetVersion) AS dataSetVersion,
                    FIRST(dataSetTitle) AS dataSetTitle,
                    FIRST(resultsCount) AS resultsCount,
                    FIRST(totalRowsCount) AS totalRowsCount,
                    FIRST(query) AS query,
                    CAST(COUNT(queryHash) AS INT) AS queryExecutions
                FROM sourceTable
                GROUP BY queryVersionHash
                ORDER BY queryVersionHash"
            );

            await connection.ExecuteNonQueryAsync(
                @"
                CREATE TABLE queryAccessReport AS 
                SELECT 
                    queryVersionHash,
                    dataSetVersionId,
                    startTime,
                    endTime,
                    CAST(EXTRACT('milliseconds' FROM EndTime - StartTime) AS INT) AS durationMillis,
                    previewToken->>'label' AS previewTokenLabel,
                    CAST(previewToken->>'dataSetVersionId' AS UUID) AS previewTokenDataSetVersionId,
                    CAST(previewToken->>'created' AS DATETIME) AS previewTokenCreated,
                    CAST(previewToken->>'expiry' AS DATETIME) AS previewTokenExpiry,
                    requestedDataSetVersion
                FROM sourceTable
                ORDER BY queryHash, startTime
            "
            );

            var queryReportFilePath =
                $"{reportsFolderPathAndFilenamePrefix}_public-api-queries.parquet";

            await connection.ExecuteNonQueryAsync(
                $@"
                COPY (SELECT * FROM queryReport)
                TO '{queryReportFilePath}' (FORMAT 'parquet', CODEC 'zstd')"
            );

            var queryAccessReportFilePath =
                $"{reportsFolderPathAndFilenamePrefix}_public-api-query-access.parquet";

            await connection.ExecuteNonQueryAsync(
                $@"
                COPY (SELECT * FROM queryAccessReport)
                TO '{queryAccessReportFilePath}' (FORMAT 'parquet', CODEC 'zstd')"
            );
        }
    }
}
