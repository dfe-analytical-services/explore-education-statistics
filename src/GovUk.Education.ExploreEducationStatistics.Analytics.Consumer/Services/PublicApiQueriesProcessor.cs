using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
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
            initialiseAction: InitialiseDuckDb,
            processSourceFileAction: ProcessSourceFile,
            createParquetReportsAction: CreateParquetReports,
            logger: logger);

        return workflow.Process();
    }

    private static void InitialiseDuckDb(InitialiseDuckDbContext context)
    {
        context.Connection.ExecuteNonQuery(@"
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

    private static void ProcessSourceFile(ProcessSourceFileContext context)
    {
        context.Connection.ExecuteNonQuery($@"
            INSERT INTO queries BY NAME (
                SELECT
                    MD5(CONCAT(query, dataSetVersionId)) AS queryVersionHash,
                    MD5(query) AS queryHash,
                    *
                FROM read_json('{context.SourceFilePath}', 
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

    private static void CreateParquetReports(CreateParquetReportsContext context)
    {
        context.Connection.ExecuteNonQuery(@"
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
    
        context.Connection.ExecuteNonQuery(@"
            CREATE TABLE queryAccessReport AS 
            SELECT 
                queryVersionHash,
                dataSetVersionId,
                startTime,
                endTime,
                CAST(EXTRACT('milliseconds' FROM EndTime - StartTime) AS INT) AS durationMillis
            FROM queries
            ORDER BY queryHash, startTime");

        var queryReportFilePath = 
            $"{context.ReportsFilePathAndFilenamePrefix}_public-api-queries.parquet";
    
        context.Connection.ExecuteNonQuery($@"
            COPY (SELECT * FROM queryReport)
            TO '{queryReportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");

        var queryAccessReportFilePath = 
            $"{context.ReportsFilePathAndFilenamePrefix}_public-api-query-access.parquet";

        context.Connection.ExecuteNonQuery($@"
            COPY (SELECT * FROM queryAccessReport)
            TO '{queryAccessReportFilePath}' (FORMAT 'parquet', CODEC 'zstd')");
    }
}
