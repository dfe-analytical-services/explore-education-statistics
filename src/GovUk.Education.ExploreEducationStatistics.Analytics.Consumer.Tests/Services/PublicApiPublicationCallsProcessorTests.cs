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

public abstract class PublicApiPublicationCallsProcessorTests : ProcessorTestsBase
{
    private readonly string _queryResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApi",
        "PublicationCalls");

    public class ProcessTests : PublicApiPublicationCallsProcessorTests
    {
        [Fact]
        public async Task CorePublicationDetails_CapturedInReport()
        {
            SetupRequestFile(PathResolver, "WithCorePublicationDetails.json");

            var service = BuildService();
            await service.Process();

            // The root processing folder is safe to leave behind.
            Assert.True(Directory.Exists(ProcessingDirectoryPath(PathResolver)));
            
            // The temporary processing folder that was set up for this run of the processor
            // should have been cleared away.
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(PathResolver)));
            
            Assert.True(Directory.Exists(PathResolver.PublicApiPublicationCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(PathResolver.PublicApiPublicationCallsReportsDirectoryPath());

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
                expectedType: "GetSummary",
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                expectedParameters: null);
        }
        
        [Fact]
        public async Task WithParameters_CapturedInReport()
        {
            SetupRequestFile(PathResolver, "WithParameters.json");

            var service = BuildService();
            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(PathResolver)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(PathResolver)));
            Assert.True(Directory.Exists(PathResolver.PublicApiPublicationCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(PathResolver.PublicApiPublicationCallsReportsDirectoryPath());

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
                expectedType: "GetDataSets",
                expectedStartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                expectedParameters: """{"page":1,"pageSize":10}""");
        }
        
        [Fact]
        public async Task MultipleCalls_CapturedInReport()
        {
            SetupRequestFile(PathResolver, "WithCorePublicationDetails.json");
            SetupRequestFile(PathResolver, "WithParameters.json");

            var service = BuildService();
            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(PathResolver)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(PathResolver)));
            Assert.True(Directory.Exists(PathResolver.PublicApiPublicationCallsReportsDirectoryPath()));

            var reports = Directory.GetFiles(PathResolver.PublicApiPublicationCallsReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the 2 recorded queries have resulted in 2 lines in the query report
            // and the order is in ascending date order.
            Assert.Equal(2, reportRows.Count);
            
            AssertReportRowOk(
                reportRows[0],
                expectedType: "GetDataSets",
                expectedStartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                expectedParameters: """{"page":1,"pageSize":10}""");
            
            AssertReportRowOk(
                reportRows[1],
                expectedType: "GetSummary",
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                expectedParameters: null);
        }

        private static async Task<List<QueryReportLine>> ReadReport(
            DuckDbConnection duckDbConnection,
            string queryReportFile)
        {
            return (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{queryReportFile:raw}')")
                    .QueryAsync<QueryReportLine>())
                .OrderBy(row => row.PublicationTitle)
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
        Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.PublicationId);
        Assert.Equal("Publication 1", queryReportRow.PublicationTitle);
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

    private PublicApiPublicationCallsProcessor BuildService()
    {
        return new PublicApiPublicationCallsProcessor(
            pathResolver: PathResolver,
            workflow: Workflow);
    }

    private void SetupRequestFile(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiPublicationCallsDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiPublicationCallsDirectoryPath(), filename));
    }
    
    private static string ProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(pathResolver.PublicApiPublicationCallsDirectoryPath(), "processing");
    }
    
    private static string TemporaryProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(ProcessingDirectoryPath(pathResolver), "temp-processing-folder");
    }
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine
    {
        public Guid PublicationId { get; init; }
        public string PublicationTitle { get; init; } = string.Empty;
        public string? Parameters { get; init; }
        public DateTime StartTime { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
