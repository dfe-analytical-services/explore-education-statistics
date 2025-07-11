using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicApiTopLevelCallsProcessorTests
{
    private readonly string _queryResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApi",
        "TopLevelCalls");

    public class ProcessTests : PublicApiTopLevelCallsProcessorTests
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
            SetupRequestFile(pathResolver, "WithCoreTopLevelDetails.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            // The root processing folder is safe to leave behind.
            Assert.True(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            
            // The temporary processing folder that was set up for this run of the processor
            // should have been cleared away.
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(pathResolver)));
            
            Assert.True(Directory.Exists(pathResolver.PublicApiTopLevelCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiTopLevelCallsReportsDirectoryPath());

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
                expectedType: "GetPublications",
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: null);
        }
        
        [Fact]
        public async Task WithParameters_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithParameters.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiTopLevelCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiTopLevelCallsReportsDirectoryPath());

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
                expectedType: "GetPublications",
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: """{"page":"1","pageSize":"10"}""");
        }

        [Fact]
        public async Task MultipleCalls_CapturedInReport()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "WithCoreTopLevelDetails.json");
            SetupRequestFile(pathResolver, "WithParameters.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicApiTopLevelCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiTopLevelCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the 2 recorded queries have resulted in 2 lines in the query report
            // and the order is in ascending date order.
            Assert.Equal(2, reportRows.Count);

            AssertReportRowOk(
                reportRows[0],
                expectedType: "GetPublications",
                expectedStartTime: DateTime.Parse("2025-02-24T02:07:44.850Z"),
                expectedParameters: """{"page":"1","pageSize":"10"}""");

            AssertReportRowOk(
                reportRows[1],
                expectedType: "GetPublications",
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedParameters: null);
        }

        private static async Task<List<QueryReportLine>> ReadReport(DuckDbConnection duckDbConnection, string queryReportFile)
        {
            return (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{queryReportFile:raw}')")
                    .QueryAsync<QueryReportLine>())
                .OrderBy(row => row.StartTime)
                .ToList();
        }
    }
    
    private static void AssertReportRowOk(
        QueryReportLine queryReportRow,
        string expectedType,
        DateTime expectedStartTime,
        string? expectedParameters)
    {
        Assert.Equal(expectedType, queryReportRow.Type);
        Assert.Equal(expectedStartTime, queryReportRow.StartTime);

        if (expectedParameters != null)
        {
            Assert.Equal(expectedParameters, queryReportRow.Parameters);
        }
        else
        {
            Assert.Null(queryReportRow.Parameters);
        }
    }

    private PublicApiTopLevelCallsProcessor BuildService(
        TestAnalyticsPathResolver pathResolver,
        IProcessRequestFilesWorkflow? workflow = null)
    {
        return new PublicApiTopLevelCallsProcessor(
            pathResolver: pathResolver,
            workflow: workflow ?? new ProcessRequestFilesWorkflow(
                logger: Mock.Of<ILogger<ProcessRequestFilesWorkflow>>(),
                fileAccessor: new FilesystemFileAccessor(),
                dateTimeProvider: new DateTimeProvider(),
                temporaryProcessingFolderNameGenerator: () => "temp-processing-folder"));
    }

    private void SetupRequestFile(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiTopLevelCallsDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiTopLevelCallsDirectoryPath(), filename));
    }
    
    private static string ProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(pathResolver.PublicApiTopLevelCallsDirectoryPath(), "processing");
    }
    
    private static string TemporaryProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(ProcessingDirectoryPath(pathResolver), "temp-processing-folder");
    }
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine
    {
        public string? Parameters { get; init; }
        public DateTime StartTime { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
