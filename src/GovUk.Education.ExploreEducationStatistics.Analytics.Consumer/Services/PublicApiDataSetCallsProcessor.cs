using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiDataSetCallsProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicApiDataSetCallsProcessor> logger,
    IWorkflowActor<PublicApiDataSetCallsProcessor>? workflowActor = null) : IRequestFileProcessor
{
    private readonly IWorkflowActor<PublicApiDataSetCallsProcessor> _workflowActor 
        = workflowActor ?? new WorkflowActor();
    
    public Task Process()
    {
        var workflow = new ProcessRequestFilesWorkflow<PublicApiDataSetCallsProcessor>(
            sourceDirectory: pathResolver.PublicApiDataSetCallsDirectoryPath(),
            reportsDirectory: pathResolver.PublicApiDataSetCallsReportsDirectoryPath(),
            actor: _workflowActor,
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor<PublicApiDataSetCallsProcessor>
    {
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
                $"{reportsFolderPathAndFilenamePrefix}_public-api-data-set-version-calls.parquet";
        
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
