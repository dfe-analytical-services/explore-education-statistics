using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;
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
        public async Task ProcessorUsesWorkflow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithCoreDataSetVersionDetails.json");

            var workflowActorBuilder = new WorkflowActorMockBuilder<PublicApiDataSetVersionCallsProcessor>();
            
            var workflowActor = workflowActorBuilder 
                .WhereDuckDbInitialisedWithErrors()
                .Build();

            var service = BuildService(
                pathResolver: pathResolver,
                workflowActor: workflowActor);
            
            await Assert.ThrowsAsync<ArgumentException>(service.Process);

            workflowActorBuilder
                .Assert
                .InitialiseDuckDbCalled();
        }

        [Fact]
        public async Task CoreDataSetVersionDetails_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithCoreDataSetVersionDetails.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

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
                expectedType: "GetDataSetVersionMetadata",
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
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

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
                expectedType: "GetDataSetVersionSummary",
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
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

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
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: """{"types":["Filters","Indicators","Locations","TimePeriods"]}""");
        }
        
        [Fact]
        public async Task MultipleCalls_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithCoreDataSetVersionDetails.json");
            SetupRequestFile(pathResolver, "WithParameters.json");
            SetupRequestFile(pathResolver, "WithPreviewTokens.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiDataSetVersionCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the 3 recorded queries have resulted in 3 lines in the query report
            // and the order is in ascending date order.
            Assert.Equal(3, reportRows.Count);
            
            AssertReportRowOk(
                reportRows[0],
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: null);
            
            AssertReportRowOk(
                reportRows[1],
                expectedType: "GetDataSetVersionMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: """{"types":["Filters","Indicators","Locations","TimePeriods"]}""");
            
            AssertReportRowOk(
                reportRows[2],
                expectedType: "GetDataSetVersionSummary",
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

    private PublicApiDataSetVersionCallsProcessor BuildService(
        TestAnalyticsPathResolver pathResolver,
        IWorkflowActor<PublicApiDataSetVersionCallsProcessor>? workflowActor = null)
    {
        return new PublicApiDataSetVersionCallsProcessor(
            pathResolver: pathResolver,
            logger: Mock.Of<ILogger<PublicApiDataSetVersionCallsProcessor>>(),
            workflowActor: workflowActor);
    }

    private void SetupRequestFile(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiDataSetVersionCallsDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiDataSetVersionCallsDirectoryPath(), filename));
    }
    
    private static string ProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(pathResolver.PublicApiDataSetVersionCallsDirectoryPath(), "processing");
    }
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine
    {
        public Guid DataSetId { get; init; }
        public string DataSetTitle { get; init; } = string.Empty;
        public string DataSetVersion { get; init; } = string.Empty;
        public Guid DataSetVersionId { get; init; }
        public string? Parameters { get; init; }
        public string RequestedDataSetVersion { get; init; } = string.Empty;
        public string? PreviewTokenLabel { get; init; }
        public Guid? PreviewTokenDataSetVersionId { get; init; }
        public DateTime? PreviewTokenCreated { get; init; }
        public DateTime? PreviewTokenExpiry { get; init; }
        public DateTime StartTime { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
