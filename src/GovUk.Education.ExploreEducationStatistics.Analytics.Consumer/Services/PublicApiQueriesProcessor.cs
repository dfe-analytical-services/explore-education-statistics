using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;

public class PublicApiQueriesProcessor(
    IAnalyticsPathResolver pathResolver,
    ILogger<PublicApiQueriesProcessor> logger) : IRequestFileProcessor
{
    public Task Process()
    {
        var workflow = new ProcessRequestFilesWorkflow(
            processorName: GetType().Name,
            sourceDirectory: pathResolver.PublicApiQueriesDirectoryPath(),
            reportsDirectory: pathResolver.PublicApiQueriesReportsDirectoryPath(),
            actor: new WorkflowActor(),
            logger: logger);

        return workflow.Process();
    }

    private class WorkflowActor : IWorkflowActor
    {
        public void InitialiseDuckDb(DuckDbConnection connection)
        {
            connection.ExecuteNonQuery(@"
                CREATE TABLE queries (
                    queryVersionHash VARCHAR,
                    queryHash VARCHAR,
                    dataSetId UUID,
                    dataSetVersionId UUID,
                    dataSetVersion VARCHAR,
                    dataSetTitle VARCHAR,
                    resultsCount INT,
                    totalRowsCount INT,
                    startTime DATETIME,
                    endTime DATETIME,
                    query JSON
                );
            ");
        }

        public void ProcessSourceFile(string sourceFilePath, DuckDbConnection connection)
        {
            connection.ExecuteNonQuery($@"
                INSERT INTO queries BY NAME (
                    SELECT
                        MD5(CONCAT(query, dataSetVersionId)) AS queryVersionHash,
                        MD5(query) AS queryHash,
                        *
                    FROM read_json('{sourceFilePath}', 
                        format='unstructured',
                        columns = {{
                            dataSetId: UUID, 
                            dataSetVersionId: UUID,
                            dataSetVersion: VARCHAR,
                            dataSetTitle: VARCHAR,
                            resultsCount: INT,
                            totalRowsCount: INT,
                            startTime: DATETIME,
                            endTime: DATETIME,
                            query: JSON
                        }})
                   )
            ");
        }

        public void CreateParquetReports(string reportsFilePathAndFilenamePrefix, DuckDbConnection connection)
        {
            connection.ExecuteNonQuery(@"
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
                FROM queries
                GROUP BY queryVersionHash
                ORDER BY queryVersionHash");
        
                connection.ExecuteNonQuery(@"
                CREATE TABLE queryAccessReport AS 
                SELECT 
                    queryVersionHash,
                    dataSetVersionId,
                    startTime,
                    endTime,
                    CAST(EXTRACT('milliseconds' FROM EndTime - StartTime) AS INT) AS durationMillis
                FROM queries
                ORDER BY queryHash, startTime
            ");

            var queryReportFilePath = 
                $"{reportsFilePathAndFilenamePrefix}_public-api-queries.parquet";
    
            connection.ExecuteNonQuery($@"
                COPY (SELECT * FROM queryReport)
                TO '{queryReportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");

            var queryAccessReportFilePath = 
                $"{reportsFilePathAndFilenamePrefix}_public-api-query-access.parquet";

            connection.ExecuteNonQuery($@"
                COPY (SELECT * FROM queryAccessReport)
                TO '{queryAccessReportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
        }
    }
}
