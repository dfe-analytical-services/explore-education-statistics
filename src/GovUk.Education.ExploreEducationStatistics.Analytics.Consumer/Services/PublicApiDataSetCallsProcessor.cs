using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiDataSetCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    IProcessRequestFilesWorkflow workflow) : IRequestFileProcessor
{
    public Task Process()
    {
        return workflow.Process(new WorkflowActor(
            sourceDirectory: pathResolver.PublicApiDataSetCallsDirectoryPath(),
            reportsDirectory: pathResolver.PublicApiDataSetCallsReportsDirectoryPath()));
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
                CREATE TABLE DataSetCalls (
                    dataSetId UUID,
                    dataSetTitle VARCHAR,
                    parameters JSON,
                    previewToken JSON,
                    startTime DATETIME,
                    type VARCHAR
                );
            ");
        }

        public async Task ProcessSourceFile(string sourceFilePath, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync($@"
                INSERT INTO DataSetCalls BY NAME (
                    SELECT *
                    FROM read_json('{sourceFilePath}', 
                        format='unstructured'
                    )
                 )
            ");
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
                    FROM DataSetCalls 
                    ORDER BY startTime ASC
                )
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')
            ");
        }
    }
}
