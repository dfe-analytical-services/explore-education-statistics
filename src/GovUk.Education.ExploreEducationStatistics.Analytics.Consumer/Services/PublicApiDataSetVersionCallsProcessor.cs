using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
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
            initialiseAction: InitialiseDuckDb,
            processSourceFileAction: ProcessSourceFile,
            createParquetReportsAction: CreateParquetReports,
            logger: logger);

        return workflow.Process();
    }

    private static void InitialiseDuckDb(InitialiseDuckDbContext context)
    {
        context.Connection.ExecuteNonQuery(@"
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

    private static void ProcessSourceFile(ProcessSourceFileContext context)
    {
        context.Connection.ExecuteNonQuery($@"
            INSERT INTO DataSetVersionCalls BY NAME (
                SELECT *
                FROM read_json('{context.SourceFilePath}', 
                    format='unstructured'
                )
             )
        ");
    }

    private static void CreateParquetReports(CreateParquetReportsContext context)
    {
        var reportFilePath = 
            $"{context.ReportsFilePathAndFilenamePrefix}_public-api-data-set-version-calls.parquet";
        
        context.Connection.ExecuteNonQuery($@"
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
