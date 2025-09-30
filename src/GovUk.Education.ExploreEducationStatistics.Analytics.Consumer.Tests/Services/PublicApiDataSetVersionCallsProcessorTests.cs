using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicApiDataSetVersionCallsProcessorTests : ProcessorTestsBase
{
    protected override string ResourcesPath => Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApi",
        "DataSetVersionCalls");

    public class ProcessTests : PublicApiDataSetVersionCallsProcessorTests
    {
        [Fact]
        public async Task CoreDataSetVersionDetails_CapturedInReport()
        {
            var service = BuildService();
            SetupRequestFile(service, "WithCoreDataSetVersionDetails.json");

            await service.Process();

            // The root processing folder is safe to leave behind.
            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));

            // The temporary processing folder that was set up for this run of the processor
            // should have been cleared away.
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));

            Assert.True(Directory.Exists(service.ReportsDirectory));
            var reports = Directory.GetFiles(service.ReportsDirectory);

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
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                expectedRequestedDataSetVersion: null,
                expectedParameters: null);
        }

        [Fact]
        public async Task WithPreviewTokenAndRequestedDataSetVersion_CapturedInReport()
        {
            var service = BuildService();
            SetupRequestFile(service, "WithPreviewTokenAndRequestedDataSetVersion.json");

            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));
            Assert.True(Directory.Exists(service.ReportsDirectory));

            var reports = Directory.GetFiles(service.ReportsDirectory);
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
                expectedType: "DownloadCsv",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                expectedRequestedDataSetVersion: "1.*",
                expectedParameters: null);
        }

        [Fact]
        public async Task WithParameters_CapturedInReport()
        {
            var service = BuildService();
            SetupRequestFile(service, "WithParameters.json");

            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));
            Assert.True(Directory.Exists(service.ReportsDirectory));

            var reports = Directory.GetFiles(service.ReportsDirectory);
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
                expectedType: "GetMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedRequestedDataSetVersion: null,
                expectedParameters: """{"types":["Filters","Indicators","Locations","TimePeriods"]}""");
        }

        [Fact]
        public async Task MultipleCalls_CapturedInReport()
        {
            var service = BuildService();
            SetupRequestFile(service, "WithCoreDataSetVersionDetails.json");
            SetupRequestFile(service, "WithParameters.json");
            SetupRequestFile(service, "WithPreviewTokenAndRequestedDataSetVersion.json");

            await service.Process();

            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));
            Assert.True(Directory.Exists(service.ReportsDirectory));

            var reports = Directory.GetFiles(service.ReportsDirectory);
            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadReport(duckDbConnection, queryReportFile);

            // Check that the 3 recorded queries have resulted in 3 lines in the query report
            // and the order is in ascending date order.
            Assert.Equal(3, reportRows.Count);

            AssertReportRowOk(
                reportRows[0],
                expectedType: "GetMetadata",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                expectedRequestedDataSetVersion: null,
                expectedParameters: """{"types":["Filters","Indicators","Locations","TimePeriods"]}""");

            AssertReportRowOk(
                reportRows[1],
                expectedType: "DownloadCsv",
                expectPreviewToken: true,
                expectedStartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                expectedRequestedDataSetVersion: "1.*",
                expectedParameters: null);

            AssertReportRowOk(
                reportRows[2],
                expectedType: "GetSummary",
                expectPreviewToken: false,
                expectedStartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                expectedRequestedDataSetVersion: null,
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
        string? expectedRequestedDataSetVersion,
        string? expectedParameters)
    {
        Assert.Equal(expectedType, queryReportRow.Type);
        Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.DataSetId);
        Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow.DataSetVersionId);
        Assert.Equal("1.2.0", queryReportRow.DataSetVersion);
        Assert.Equal(expectedRequestedDataSetVersion, queryReportRow.RequestedDataSetVersion);
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

    private PublicApiDataSetVersionCallsProcessor BuildService()
    {
        return new PublicApiDataSetVersionCallsProcessor(
            pathResolver: PathResolver,
            workflow: Workflow);
    }

    private record QueryReportLine
    {
        public Guid DataSetId { get; init; }
        public string DataSetTitle { get; init; } = string.Empty;
        public string DataSetVersion { get; init; } = string.Empty;
        public Guid DataSetVersionId { get; init; }
        public string? Parameters { get; init; }
        public string? RequestedDataSetVersion { get; init; }
        public string? PreviewTokenLabel { get; init; }
        public Guid? PreviewTokenDataSetVersionId { get; init; }
        public DateTime? PreviewTokenCreated { get; init; }
        public DateTime? PreviewTokenExpiry { get; init; }
        public DateTime StartTime { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
