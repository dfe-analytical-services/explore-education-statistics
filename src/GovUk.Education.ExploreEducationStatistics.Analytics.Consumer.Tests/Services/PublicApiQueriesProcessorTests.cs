using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicApiQueriesProcessorTests
{
    private readonly string _queryResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApiQueries");

    public class ProcessTests : PublicApiQueriesProcessorTests
    {
        [Fact]
        public async Task NoSourceFolder_NoReportsProduced()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiQueriesProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicApiQueriesReportsDirectoryPath()));
        }

        [Fact]
        public async Task NoSourceQueriesToConsume_NoReportsProduced()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            Directory.CreateDirectory(pathResolver.PublicApiQueriesDirectoryPath());

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            // Check that as there were no files to process, no working directories were
            // created as a result.
            Assert.False(Directory.Exists(pathResolver.PublicApiQueriesProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicApiQueriesReportsDirectoryPath()));
        }

        [Fact]
        public async Task SingleSourceQuery_ProducesOneReportRow()
        {
            var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "Query1Request.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiQueriesProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiQueriesReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiQueriesReportsDirectoryPath());

            Assert.Equal(2, reports.Length);

            var queryReportFile = reports.Single(file => file.EndsWith("public-api-queries.parquet"));

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var queryReportRows = await ReadQueryReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file and the calculated fields
            // match the expected values also.
            var queryReportRow = Assert.Single(queryReportRows);

            Assert.Equal("f89944c2ee4284894962724bc68a1c8e", queryReportRow.QueryVersionHash);
            Assert.Equal("a992584964c8051b6e1b167a0a8dd4e0", queryReportRow.QueryHash);
            Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.DataSetId);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow.DataSetVersionId);
            Assert.Equal("1.2.0", queryReportRow.DataSetVersion);
            Assert.Equal("Data Set 1", queryReportRow.DataSetTitle);
            Assert.Equal(44, queryReportRow.ResultsCount);
            Assert.Equal(800, queryReportRow.TotalRowsCount);
            Assert.Equal(1, queryReportRow.QueryExecutions);
            Assert.StartsWith("{\"criteria\":{\"filters\":{\"eq\":\"qOnjG\"}", queryReportRow.Query);

            var queryAccessReportFile = reports.Single(file => file.EndsWith("public-api-query-access.parquet"));

            // Check that the single recorded query has resulted in a
            // single line in the query access report and the access times
            // match the times from the original JSON file, and the calculated
            // fields match the expected values.
            var queryAccessReportRows = await duckDbConnection
                .SqlBuilder($"SELECT * FROM read_parquet('{queryAccessReportFile:raw}')")
                .QueryAsync<QueryAccessReportLine>();

            var queryAccessReportRow = Assert.Single(queryAccessReportRows);

            Assert.Equal("f89944c2ee4284894962724bc68a1c8e", queryAccessReportRow.QueryVersionHash);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryAccessReportRow.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.710Z"), queryAccessReportRow.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.850Z"), queryAccessReportRow.EndTime);
            Assert.Equal(140, queryAccessReportRow.DurationMillis);
        }

        [Fact]
        public async Task TwoDifferentSourceQueries_ProduceTwoDistinctReportRows()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            SetupQueryRequest(pathResolver, "Query1Request.json");
            SetupQueryRequest(pathResolver, "Query2Request1.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicApiQueriesReportsDirectoryPath());

            Assert.Equal(2, reports.Length);

            var queryReportFile = reports.Single(file => file.EndsWith("public-api-queries.parquet"));

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var queryReportRows = await ReadQueryReport(duckDbConnection, queryReportFile);

            // Check that the 2 different queries have resulted in 2 lines
            // in the query report and the values match the values from the
            // original JSON files and the calculated fields match the
            // expected values also.
            Assert.Equal(2, queryReportRows.Count);

            var queryReportRow1 = queryReportRows[0];

            Assert.Equal("b856e997ec5d2c7b445c71ff14859be7", queryReportRow1.QueryVersionHash);
            Assert.Equal("7145877f51cbcab16411b8a1a7bac4c3", queryReportRow1.QueryHash);
            Assert.Equal(Guid.Parse("8b9da0ae-80e4-43e8-9f39-4f670fd1a45a"), queryReportRow1.DataSetId);
            Assert.Equal(Guid.Parse("5ed5053d-92fc-49a1-b0b1-4c11f3b2c538"), queryReportRow1.DataSetVersionId);
            Assert.Equal("2.1.0", queryReportRow1.DataSetVersion);
            Assert.Equal("Data Set 2", queryReportRow1.DataSetTitle);
            Assert.Equal(20, queryReportRow1.ResultsCount);
            Assert.Equal(23, queryReportRow1.TotalRowsCount);
            Assert.Equal(1, queryReportRow1.QueryExecutions);
            Assert.StartsWith("{\"criteria\":{\"filters\":{\"in\":[\"qOnjG\"", queryReportRow1.Query);

            var queryReportRow2 = queryReportRows[1];

            Assert.Equal("f89944c2ee4284894962724bc68a1c8e", queryReportRow2.QueryVersionHash);
            Assert.Equal("a992584964c8051b6e1b167a0a8dd4e0", queryReportRow2.QueryHash);
            Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow2.DataSetId);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow2.DataSetVersionId);
            Assert.Equal("1.2.0", queryReportRow2.DataSetVersion);
            Assert.Equal("Data Set 1", queryReportRow2.DataSetTitle);
            Assert.Equal(44, queryReportRow2.ResultsCount);
            Assert.Equal(800, queryReportRow2.TotalRowsCount);
            Assert.Equal(1, queryReportRow2.QueryExecutions);
            Assert.StartsWith("{\"criteria\":{\"filters\":{\"eq\":\"qOnjG\"}", queryReportRow2.Query);

            var queryAccessReportFile = reports.Single(file => file.EndsWith("public-api-query-access.parquet"));

            // Check that the 2 different recorded queries have resulted in
            // 2 separate lines in the query access report and the access times
            // match the times from the original JSON file, and the calculated
            // fields match the expected values.
            var queryAccessReportRows = (await duckDbConnection
                .SqlBuilder($"SELECT * FROM read_parquet('{queryAccessReportFile:raw}')")
                .QueryAsync<QueryAccessReportLine>())
                .ToList();

            Assert.Equal(2, queryAccessReportRows.Count);

            var queryAccessReportRow1 = queryAccessReportRows[0];

            Assert.Equal("b856e997ec5d2c7b445c71ff14859be7", queryAccessReportRow1.QueryVersionHash);
            Assert.Equal(Guid.Parse("5ed5053d-92fc-49a1-b0b1-4c11f3b2c538"), queryAccessReportRow1.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-23T03:07:44.931Z"), queryAccessReportRow1.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-23T03:07:44.955Z"), queryAccessReportRow1.EndTime);
            Assert.Equal(24, queryAccessReportRow1.DurationMillis);

            var queryAccessReportRow2 = queryAccessReportRows[1];

            Assert.Equal("f89944c2ee4284894962724bc68a1c8e", queryAccessReportRow2.QueryVersionHash);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryAccessReportRow2.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.710Z"), queryAccessReportRow2.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.850Z"), queryAccessReportRow2.EndTime);
            Assert.Equal(140, queryAccessReportRow2.DurationMillis);
        }

        [Fact]
        public async Task MultipleSourceFilesForSameQuery_ProduceSingleQueryRowAndMultipleQueryAccessRows()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            SetupQueryRequest(pathResolver, "Query2Request1.json");
            SetupQueryRequest(pathResolver, "Query2Request2.json");
            SetupQueryRequest(pathResolver, "Query2Request3.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicApiQueriesReportsDirectoryPath());

            Assert.Equal(2, reports.Length);

            var queryReportFile = reports.Single(file => file.EndsWith("public-api-queries.parquet"));

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var queryReportRows = (await duckDbConnection
                .SqlBuilder($"SELECT * FROM read_parquet('{queryReportFile:raw}')")
                .QueryAsync<QueryReportLine>())
                    .ToList();

            // Check that the 3 different source files for the same query
            // have resulted in a single line in the query report, and the
            // values match the values from the original JSON files and the
            // calculated fields match the expected values also.
            var queryReportRow = Assert.Single(queryReportRows);

            Assert.Equal("b856e997ec5d2c7b445c71ff14859be7", queryReportRow.QueryVersionHash);
            Assert.Equal("7145877f51cbcab16411b8a1a7bac4c3", queryReportRow.QueryHash);
            Assert.Equal(Guid.Parse("8b9da0ae-80e4-43e8-9f39-4f670fd1a45a"), queryReportRow.DataSetId);
            Assert.Equal(Guid.Parse("5ed5053d-92fc-49a1-b0b1-4c11f3b2c538"), queryReportRow.DataSetVersionId);
            Assert.Equal("2.1.0", queryReportRow.DataSetVersion);
            Assert.Equal("Data Set 2", queryReportRow.DataSetTitle);
            Assert.Equal(20, queryReportRow.ResultsCount);
            Assert.Equal(23, queryReportRow.TotalRowsCount);

            // Check that the function successfully spotted that 3 instances of this query
            // were found in this batch of requests.
            Assert.Equal(3, queryReportRow.QueryExecutions);
            Assert.StartsWith("{\"criteria\":{\"filters\":{\"in\":[\"qOnjG\"", queryReportRow.Query);

            var queryAccessReportFile = reports.Single(file => file.EndsWith("public-api-query-access.parquet"));

            var queryAccessReportRows = (await duckDbConnection
                .SqlBuilder($"SELECT * FROM read_parquet('{queryAccessReportFile:raw}')")
                .QueryAsync<QueryAccessReportLine>())
                .ToList();

            // Check that the 3 different recorded query accesses have resulted in
            // 3 separate lines in the query access report and the access times
            // match the times from the original JSON file, and the calculated
            // fields match the expected values.
            //
            // Also check that the accesses were recorded in order of start date.
            Assert.Equal(3, queryAccessReportRows.Count);

            var queryAccessReportRow1 = queryAccessReportRows[0];

            // Check that the first query access was recorded.
            Assert.Equal("b856e997ec5d2c7b445c71ff14859be7", queryAccessReportRow1.QueryVersionHash);
            Assert.Equal(Guid.Parse("5ed5053d-92fc-49a1-b0b1-4c11f3b2c538"), queryAccessReportRow1.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-22T03:07:44.931Z"), queryAccessReportRow1.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-22T03:07:44.955Z"), queryAccessReportRow1.EndTime);
            Assert.Equal(24, queryAccessReportRow1.DurationMillis);

            var queryAccessReportRow2 = queryAccessReportRows[1];

            // Check that the second query access was recorded.
            Assert.Equal("b856e997ec5d2c7b445c71ff14859be7", queryAccessReportRow2.QueryVersionHash);
            Assert.Equal(Guid.Parse("5ed5053d-92fc-49a1-b0b1-4c11f3b2c538"), queryAccessReportRow2.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-23T03:07:44.931Z"), queryAccessReportRow2.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-23T03:07:44.955Z"), queryAccessReportRow2.EndTime);
            Assert.Equal(24, queryAccessReportRow2.DurationMillis);

            var queryAccessReportRow3 = queryAccessReportRows[2];

            // Check that the third query access was recorded.
            Assert.Equal("b856e997ec5d2c7b445c71ff14859be7", queryAccessReportRow3.QueryVersionHash);
            Assert.Equal(Guid.Parse("5ed5053d-92fc-49a1-b0b1-4c11f3b2c538"), queryAccessReportRow3.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.931Z"), queryAccessReportRow3.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.955Z"), queryAccessReportRow3.EndTime);
            Assert.Equal(24, queryAccessReportRow3.DurationMillis);
        }

        [Fact]
        public async Task SameQueryStructureButDifferentDataSetVersion_ProducesTwoDistinctReportRows()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            SetupQueryRequest(pathResolver, "Query1Request.json");
            SetupQueryRequest(pathResolver, "Query1RequestMinorVersionUpdate.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicApiQueriesReportsDirectoryPath());

            Assert.Equal(2, reports.Length);

            var queryReportFile = reports.Single(file => file.EndsWith("public-api-queries.parquet"));

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var queryReportRows = await ReadQueryReport(duckDbConnection, queryReportFile);

            // Check that the 2 different data set versions result in 2 distinct entries in the
            // report file, despite their queries being structurally the same and for the same
            // overarching data set.
            Assert.Equal(2, queryReportRows.Count);

            var queryReportRow1 = queryReportRows[0];

            Assert.Equal("34822b182432c266b7198c243245fd60", queryReportRow1.QueryVersionHash);
            Assert.Equal("a992584964c8051b6e1b167a0a8dd4e0", queryReportRow1.QueryHash);
            Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow1.DataSetId);
            Assert.Equal(Guid.Parse("68c73c8e-808d-47fd-a2d5-268dc6b3a102"), queryReportRow1.DataSetVersionId);
            Assert.Equal("1.2.1", queryReportRow1.DataSetVersion);
            Assert.Equal("Data Set 1", queryReportRow1.DataSetTitle);
            Assert.Equal(52, queryReportRow1.ResultsCount);
            Assert.Equal(850, queryReportRow1.TotalRowsCount);
            Assert.Equal(1, queryReportRow1.QueryExecutions);
            Assert.StartsWith("{\"criteria\":{\"filters\":{\"eq\":\"qOnjG\"}", queryReportRow1.Query);

            var queryReportRow2 = queryReportRows[1];

            Assert.Equal("f89944c2ee4284894962724bc68a1c8e", queryReportRow2.QueryVersionHash);
            Assert.Equal("a992584964c8051b6e1b167a0a8dd4e0", queryReportRow2.QueryHash);
            Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow2.DataSetId);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow2.DataSetVersionId);
            Assert.Equal("1.2.0", queryReportRow2.DataSetVersion);
            Assert.Equal("Data Set 1", queryReportRow2.DataSetTitle);
            Assert.Equal(44, queryReportRow2.ResultsCount);
            Assert.Equal(800, queryReportRow2.TotalRowsCount);
            Assert.Equal(1, queryReportRow2.QueryExecutions);
            Assert.StartsWith("{\"criteria\":{\"filters\":{\"eq\":\"qOnjG\"}", queryReportRow2.Query);

            var queryAccessReportFile = reports.Single(file => file.EndsWith("public-api-query-access.parquet"));

            var queryAccessReportRows = (await duckDbConnection
                .SqlBuilder($"SELECT * FROM read_parquet('{queryAccessReportFile:raw}')")
                .QueryAsync<QueryAccessReportLine>())
                .ToList();

            // Check that the 2 different recorded query accesses have resulted in
            // 2 separate lines in the query access report.
            //
            // Also check that the accesses were recorded in order of start date.
            Assert.Equal(2, queryAccessReportRows.Count);

            var queryAccessReportRow1 = queryAccessReportRows[0];

            // Check that the first query access was recorded.
            Assert.Equal("f89944c2ee4284894962724bc68a1c8e", queryAccessReportRow1.QueryVersionHash);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryAccessReportRow1.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.710Z"), queryAccessReportRow1.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.850Z"), queryAccessReportRow1.EndTime);
            Assert.Equal(140, queryAccessReportRow1.DurationMillis);

            var queryAccessReportRow2 = queryAccessReportRows[1];

            // Check that the second query access was recorded.
            Assert.Equal("34822b182432c266b7198c243245fd60", queryAccessReportRow2.QueryVersionHash);
            Assert.Equal(Guid.Parse("68c73c8e-808d-47fd-a2d5-268dc6b3a102"), queryAccessReportRow2.DataSetVersionId);
            Assert.Equal(DateTime.Parse("2025-02-25T03:07:44.710Z"), queryAccessReportRow2.StartTime);
            Assert.Equal(DateTime.Parse("2025-02-25T03:07:44.850Z"), queryAccessReportRow2.EndTime);
            Assert.Equal(140, queryAccessReportRow2.DurationMillis);
        }

        private static async Task<List<QueryReportLine>> ReadQueryReport(DuckDbConnection duckDbConnection, string queryReportFile)
        {
            var queryReportRows = (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{queryReportFile:raw}')")
                    .QueryAsync<QueryReportLine>())
                .ToList();
            return queryReportRows;
        }
    }

    private PublicApiQueriesProcessor BuildService(
        TestAnalyticsPathResolver? pathResolver = null)
    {
        return new PublicApiQueriesProcessor(
            duckDbConnection: new DuckDbConnection(),
            pathResolver: pathResolver ?? new TestAnalyticsPathResolver(),
            Mock.Of<ILogger<PublicApiQueriesProcessor>>());
    }

    private void SetupQueryRequest(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiQueriesDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiQueriesDirectoryPath(), filename));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine(
        string QueryVersionHash,
        string QueryHash,
        Guid DataSetId,
        Guid DataSetVersionId,
        string DataSetVersion,
        string DataSetTitle,
        int ResultsCount,
        int TotalRowsCount,
        string Query,
        int QueryExecutions);
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryAccessReportLine(
        string QueryVersionHash,
        Guid DataSetVersionId,
        DateTime StartTime,
        DateTime EndTime,
        int DurationMillis);
}
