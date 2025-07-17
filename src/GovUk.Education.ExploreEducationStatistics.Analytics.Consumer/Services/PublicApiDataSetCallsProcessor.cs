using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Extensions;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiDataSetCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow) : IRequestFileProcessor
{
    private static readonly string[] PublicApiDataSetsSubPath = ["public-api", "data-sets"];

    public string SourceDirectory => pathResolver.BuildSourceDirectory(PublicApiDataSetsSubPath);
    public string ReportsDirectory => pathResolver.BuildReportsDirectory(PublicApiDataSetsSubPath);

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
                    dataSetId UUID,
                    dataSetTitle VARCHAR,
                    parameters JSON,
                    previewToken JSON,
                    startTime DATETIME,
                    type VARCHAR
                );
            ");
        }

        public async Task ProcessSourceFiles(string sourceFilesDirectory, DuckDbConnection connection)
        {
            await connection.DirectCopyJsonIntoDuckDbTable(
                jsonFilePath: sourceFilesDirectory,
                tableName: "sourceTable");
        }

        public async Task CreateParquetReports(string reportsFolderPathAndFilenamePrefix, DuckDbConnection connection)
        {
            var reportFilePath = 
                $"{reportsFolderPathAndFilenamePrefix}_public-api-data-set-calls.parquet";
        
            await connection.ExecuteNonQueryAsync($@"
                COPY (
                    SELECT * EXCLUDE previewToken,
                    previewToken->>'label' AS previewTokenLabel,
                    CAST(previewToken->>'dataSetVersionId' AS UUID) AS previewTokenDataSetVersionId,
                    CAST(previewToken->>'created' AS DATETIME) AS previewTokenCreated,
                    CAST(previewToken->>'expiry' AS DATETIME) AS previewTokenExpiry 
                    FROM sourceTable 
                    ORDER BY startTime ASC
                )
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')
            ");
        }
    }
}
