using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicApiDataSetCallsProcessorTests
{
    private readonly string _queryResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApi",
        "DataSetCalls");

    public class ProcessTests : PublicApiDataSetCallsProcessorTests
    {
        [Fact]
        public async Task ProcessorUsesWorkflow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            var workflow = new Mock<IProcessRequestFilesWorkflow>(MockBehavior.Strict);

            workflow
                .Setup(s => s.Process(It.IsAny<IWorkflowActor>()))
                .Returns(Task.CompletedTask);
            
            var service = BuildService(
                pathResolver: pathResolver,
                workflow: workflow.Object);
            
            await service.Process();

            workflow.Verify(s => s.Process(It.IsAny<IWorkflowActor>()), Times.Once);
        }

        [Fact]
        public async Task CoreDataSetDetails_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithCoreDataSetDetails.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);

            AssertReportRowOk(
                queryReportRow,
                expectedType: "GetDataSetSummary",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: null);
        }
        
        [Fact]
        public async Task WithPreviewTokens_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithPreviewTokens.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);

            AssertReportRowOk(
                queryReportRow,
                expectedType: "GetDataSetSummary",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                expectedParameters: null);
        }
        
        [Fact]
        public async Task WithParameters_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithParameters.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);

            AssertReportRowOk(
                queryReportRow,
                expectedType: "ListVersions",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: """{"page":"1","pageSize":"10"}""");
        }
        
        [Fact]
        public async Task MultipleCalls_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithCoreDataSetDetails.json");
            SetupRequestFile(pathResolver, "WithParameters.json");
            SetupRequestFile(pathResolver, "WithPreviewTokens.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the 3 recorded queries have resulted in 3 lines in the query report
            // and the order is in ascending date order.
            Assert.Equal(3, reportRows.Count);
            
            AssertReportRowOk(
                reportRows[0],
                expectedType: "GetDataSetSummary",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: null);
            
            AssertReportRowOk(
                reportRows[1],
                expectedType: "ListVersions",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: """{"page":"1","pageSize":"10"}""");
            
            AssertReportRowOk(
                reportRows[2],
                expectedType: "GetDataSetSummary",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                expectedParameters: null);
        }

        private static async Task<List<QueryReportLine>> ReadReport(DuckDbConnection duckDbConnection, string queryReportFile)
        {
            return (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{queryReportFile:raw}')")
                    .QueryAsync<QueryReportLine>())
                .OrderBy(row => row.DataSetTitle)
                .ToList();
        }
    }
    
    private static void AssertReportRowOk(
        QueryReportLine queryReportRow,
        string expectedType,
        DateTime expectedStartTime,
        bool expectPreviewToken,
        string? expectedParameters)
    {
        Assert.Equal(expectedType, queryReportRow.Type);
        Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.DataSetId);
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
            Assert.Equal("Preview token content", queryReportRow.PreviewTokenLabel);
            Assert.Equal(DateTime.Parse("2025-02-23T11:02:44.850Z"), queryReportRow.PreviewTokenCreated);
            Assert.Equal(DateTime.Parse("2025-02-24T11:02:44.850Z"), queryReportRow.PreviewTokenExpiry);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow.PreviewTokenDataSetVersionId);
        }
        else
        {
            Assert.Null(queryReportRow.PreviewTokenLabel);
            Assert.Null(queryReportRow.PreviewTokenCreated);
            Assert.Null(queryReportRow.PreviewTokenExpiry);
            Assert.Null(queryReportRow.PreviewTokenDataSetVersionId);
        }
    }

    private PublicApiDataSetCallsProcessor BuildService(
        TestAnalyticsPathResolver pathResolver,
        IProcessRequestFilesWorkflow? workflow = null)
    {
        return new PublicApiDataSetCallsProcessor(
            pathResolver: pathResolver,
            workflow: workflow ?? new ProcessRequestFilesWorkflow(
                logger: Mock.Of<ILogger<ProcessRequestFilesWorkflow>>()));
    }

    private void SetupRequestFile(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiDataSetCallsDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiDataSetCallsDirectoryPath(), filename));
    }
    
    private static string ProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(pathResolver.PublicApiDataSetCallsDirectoryPath(), "processing");
    }
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine
    {
        public Guid DataSetId { get; init; }
        public string DataSetTitle { get; init; } = string.Empty;
        public string? Parameters { get; init; }
        public string? PreviewTokenLabel { get; init; }
        public Guid? PreviewTokenDataSetVersionId { get; init; }
        public DateTime? PreviewTokenCreated { get; init; }
        public DateTime? PreviewTokenExpiry { get; init; }
        public DateTime StartTime { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
