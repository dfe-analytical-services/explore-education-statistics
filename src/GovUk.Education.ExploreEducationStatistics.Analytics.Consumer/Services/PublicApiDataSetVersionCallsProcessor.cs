using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiDataSetVersionCallsProcessor(
    DuckDbConnection duckDbConnection,
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicApiDataSetVersionCallsProcessor> logger)
    : IRequestFileProcessor
{
    public Task Process()
    {
        var workflowManager = new ProcessorWorkflowManager(
            duckDbConnection: duckDbConnection,
            processor: new PublicApiDataSetVersionCallsWorkflowProcessor(
                sourceDirectory: pathResolver.PublicApiDataSetVersionCallsDirectoryPath(),
                reportsDirectory: pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()),
            logger: logger);

        return workflowManager.ProcessWorkflow();
    }

    private class PublicApiDataSetVersionCallsWorkflowProcessor(
        string sourceDirectory,
        string reportsDirectory)
        : ICommonWorkflowRequestFileProcessor
    {
        public string SourceDirectory() => sourceDirectory;

        public string ReportsDirectory() => reportsDirectory;

        public string ReportsFilenameSuffix() => "public-api-data-set-version-calls";

        public void InitialiseDuckDb(DuckDbConnection connection)
        {
            connection.ExecuteNonQuery(@"
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

        public void ProcessSourceFile(string filepath, DuckDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                INSERT INTO DataSetVersionCalls BY NAME (
                    SELECT *
                    FROM read_json('{filepath}', 
                        format='unstructured'
                    )
                 )
            ");
        }

        public void CreateParquetReport(string reportFilepath, DuckDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                COPY (
                    SELECT * EXCLUDE previewToken,
                    previewToken->>'label' AS previewTokenLabel,
                    CAST(previewToken->>'dataSetVersionId' AS UUID) AS previewTokenDataSetVersionId,
                    CAST(previewToken->>'created' AS DATETIME) AS previewTokenCreated,
                    CAST(previewToken->>'expiry' AS DATETIME) AS previewTokenExpiry 
                    FROM DataSetVersionCalls 
                    ORDER BY startTime ASC
                )
                TO '{reportFilepath}' (FORMAT 'parquet', CODEC 'zstd')
            ");
        }
    }
}
