using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiDataSetVersionCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicApiDataSetVersionCallsProcessor> logger)
    : IRequestFileProcessor
{
    public Task Process()
    {
        var workflow = new ProcessRequestFilesWorkflow(
            processorName: GetType().Name,
            sourceDirectory: pathResolver.PublicApiDataSetVersionCallsDirectoryPath(),
            reportsDirectory: pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath(),
            actor: new WorkflowActor(),
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor
    {
        public async Task InitialiseDuckDb(DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync(@"
                CREATE TABLE DataSetVersionCalls (
                    dataSetId UUID,
                    dataSetTitle VARCHAR,
                    dataSetVersion VARCHAR,
                    dataSetVersionId UUID,
                    parameters JSON,
                    previewToken JSON,
                    requestedDataSetVersion VARCHAR,
                    startTime DATETIME,
                    type VARCHAR
                );
            ");
        }

        public async Task ProcessSourceFile(string sourceFilePath, DuckDbConnection connection)
        {
            await connection.ExecuteNonQueryAsync($@"
                INSERT INTO DataSetVersionCalls BY NAME (
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
                $"{reportsFolderPathAndFilenamePrefix}_public-api-data-set-version-calls.parquet";
        
            await connection.ExecuteNonQueryAsync($@"
                COPY (
                    SELECT * EXCLUDE previewToken,
                    previewToken->>'label' AS previewTokenLabel,
                    CAST(previewToken->>'dataSetVersionId' AS UUID) AS previewTokenDataSetVersionId,
                    CAST(previewToken->>'created' AS DATETIME) AS previewTokenCreated,
                    CAST(previewToken->>'expiry' AS DATETIME) AS previewTokenExpiry 
                    FROM DataSetVersionCalls 
                    ORDER BY startTime ASC
                )
                TO '{reportFilePath}' (FORMAT 'parquet', CODEC 'zstd')
            ");
        }
    }
}
