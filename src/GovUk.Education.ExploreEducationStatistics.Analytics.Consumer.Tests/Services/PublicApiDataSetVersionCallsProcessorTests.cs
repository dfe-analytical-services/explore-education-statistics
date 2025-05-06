using System.Reflection;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicApiDataSetVersionCallsProcessorTests
{
    private readonly string _queryResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApi",
        "DataSetVersionCalls");

    public class ProcessTests : PublicApiDataSetVersionCallsProcessorTests
    {
        [Fact]
        public async Task NoSourceFolder_NoReportsProduced()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));
        }

        [Fact]
        public async Task NoSourceQueriesToConsume_NoReportsProduced()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            Directory.CreateDirectory(pathResolver.PublicApiDataSetVersionCallsDirectoryPath());

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            // Check that as there were no files to process, no working directories were
            // created as a result.
            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));
        }

        [Fact]
        public async Task SingleSourceQuery_ProducesOneReportRow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "GetDataSetVersionMetadata1.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadGetMetaReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);

            AssertDataSetVersionCallReportRowOk(
                queryReportRow,
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: """{"types":["Filters","Indicators","Locations","TimePeriods"]}""");
        }

        [Fact]
        public async Task SingleSourceQuery_NoPreviewToken()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "GetDataSetVersionMetadata2NoPreviewToken.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadGetMetaReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);
            
            AssertDataSetVersionCallReportRowOk(
                queryReportRow,
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: """{"types":["Filters","Locations","TimePeriods"]}""");
        }

        [Fact]
        public async Task MultipleSourceQueries_ProducesMultipleReportRows()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "GetDataSetVersionAsCsv.json");
            SetupQueryRequest(pathResolver, "GetDataSetVersionMetadata1.json");
            SetupQueryRequest(pathResolver, "GetDataSetVersionMetadata2NoPreviewToken.json");
            SetupQueryRequest(pathResolver, "GetDataSetVersionChanges.json");
            SetupQueryRequest(pathResolver, "GetDataSetVersionSummary.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadGetMetaReport(duckDbConnection, queryReportFile);

            // Check that multiple source files produce multiple report rows.
            Assert.Equal(5, reportRows.Count);
            
            // Check that the 1st report row represents "Get Metadata" request 2,
            // as this was requested first and the report rows are ordered by "startTime".
            AssertDataSetVersionCallReportRowOk(
                reportRows[0],
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: """{"types":["Filters","Locations","TimePeriods"]}""");

            // Check that the 2nd report row represents "Get Metadata" request 1,
            // as this was requested second and the report rows are ordered by "startTime".
            AssertDataSetVersionCallReportRowOk(
                reportRows[1],
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: """{"types":["Filters","Indicators","Locations","TimePeriods"]}""");

            // Check that the 3rd report row represents the "Get Changes" request,
            // as this was requested 3rd and the report rows are ordered by "startTime".
            AssertDataSetVersionCallReportRowOk(
                reportRows[2],
                expectedType: "GetDataSetVersionChanges",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-27T03:07:44.850Z"));
            
            // Check that the 4th report row represents the "Get Summary" request,
            // as this was requested 4th and the report rows are ordered by "startTime".
            AssertDataSetVersionCallReportRowOk(
                reportRows[3],
                expectedType: "GetDataSetVersionSummary",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"));
            
            // Check that the 5th report row represents the "Get Summary" request,
            // as this was requested 5th and the report rows are ordered by "startTime".
            AssertDataSetVersionCallReportRowOk(
                reportRows[4],
                expectedType: "GetDataSetVersionAsCsv",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-03-01T03:07:44.850Z"));
        }

        private static async Task<List<QueryReportLine>> ReadGetMetaReport(DuckDbConnection duckDbConnection, string queryReportFile)
        {
            var reportRows = (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{queryReportFile:raw}')")
                    .QueryAsync<QueryReportLine>())
                .OrderBy(row => row.DataSetTitle)
                .ToList();
            return reportRows;
        }
    }
    
    private static void AssertDataSetVersionCallReportRowOk(
        QueryReportLine queryReportRow,
        string expectedType,
        DateTime expectedStartTime,
        bool expectPreviewToken,
        string? expectedParameters = null)
    {
        Assert.Equal(expectedType, queryReportRow.Type);
        Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.DataSetId);
        Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow.DataSetVersionId);
        Assert.Equal("1.2.0", queryReportRow.DataSetVersion);
        Assert.Equal("1.*", queryReportRow.RequestedDataSetVersion);
        Assert.Equal("Data Set 1", queryReportRow.DataSetTitle);
        Assert.Equal(expectedStartTime, queryReportRow.StartTime);

        if (expectedParameters != null)
        {
            Assert.Equal(expectedParameters, queryReportRow.Parameters);
        }
        else
        {
            Assert.Null(queryReportRow.Parameters);
        }

        if (expectPreviewToken)
        {
            Assert.NotNull(queryReportRow.PreviewToken);
        
            var previewToken = JsonSerializer.Deserialize<PreviewToken>(
                queryReportRow.PreviewToken, 
                options: new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            
            Assert.NotNull(previewToken);
            Assert.Equal("Preview token content", previewToken.Label);
            Assert.Equal(DateTime.Parse("2025-02-23T11:02:44.850Z"), previewToken.Created);
            Assert.Equal(DateTime.Parse("2025-02-24T11:02:44.850Z"), previewToken.Expiry);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), previewToken.DataSetVersionId);
        }
        else
        {
            Assert.Null(queryReportRow.PreviewToken);
        }
    }

    private PublicApiDataSetVersionCallsProcessor BuildService(
        TestAnalyticsPathResolver pathResolver)
    {
        return new PublicApiDataSetVersionCallsProcessor(
            duckDbConnection: new DuckDbConnection(),
            pathResolver: pathResolver,
            Mock.Of<ILogger<PublicApiDataSetVersionCallsProcessor>>());
    }

    private void SetupQueryRequest(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiDataSetVersionCallsDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiDataSetVersionCallsDirectoryPath(), filename));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine(
        Guid DataSetId,
        string DataSetTitle,
        string DataSetVersion,
        Guid DataSetVersionId,
        string Parameters,
        string PreviewToken,
        string RequestedDataSetVersion,
        DateTime StartTime,
        string Type);

    // ReSharper disable once ClassNeverInstantiated.Local
    private record PreviewToken(
        string? Label,
        DateTime Created,
        DateTime Expiry,
        Guid DataSetVersionId);
}
