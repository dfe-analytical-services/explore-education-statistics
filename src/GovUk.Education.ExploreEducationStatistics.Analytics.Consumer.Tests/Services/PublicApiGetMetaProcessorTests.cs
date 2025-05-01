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

public abstract class PublicApiGetMetaProcessorTests
{
    private readonly string _queryResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicApi",
        "GetMeta");

    public class ProcessTests : PublicApiGetMetaProcessorTests
    {
        [Fact]
        public async Task NoSourceFolder_NoReportsProduced()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaReportsDirectoryPath()));
        }

        [Fact]
        public async Task NoSourceQueriesToConsume_NoReportsProduced()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            Directory.CreateDirectory(pathResolver.PublicApiGetMetaDirectoryPath());

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            // Check that as there were no files to process, no working directories were
            // created as a result.
            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaReportsDirectoryPath()));
        }

        [Fact]
        public async Task SingleSourceQuery_ProducesOneReportRow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "Request1.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiGetMetaReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiGetMetaReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadGetMetaReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);

            AssertReportRowForRequest1Ok(queryReportRow);
        }

        [Fact]
        public async Task SingleSourceQuery_NoPreviewToken()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "Request2NoPreviewToken.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiGetMetaReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiGetMetaReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadGetMetaReport(duckDbConnection, queryReportFile);

            // Check that the single recorded query has resulted in a
            // single line in the query report and the values match the
            // values from the original JSON file.
            var queryReportRow = Assert.Single(reportRows);

            AssertReportRowForRequest2Ok(queryReportRow);
        }

        [Fact]
        public async Task MultipleSourceQueries_ProducesMultipleReportRows()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupQueryRequest(pathResolver, "Request1.json");
            SetupQueryRequest(pathResolver, "Request2NoPreviewToken.json");

            var service = BuildService(pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicApiGetMetaProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicApiGetMetaReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicApiGetMetaReportsDirectoryPath());

            var queryReportFile = Assert.Single(reports);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var reportRows = await ReadGetMetaReport(duckDbConnection, queryReportFile);

            // Check that multiple source files produce multiple report rows.
            Assert.Equal(2, reportRows.Count);
            
            // Check that the first report row represents request 2, as this was requested first
            // and the report rows are ordered by "startTime".
            var queryReportRow1 = reportRows[0];
            AssertReportRowForRequest2Ok(queryReportRow1);

            // Check that the second report row represents request 1, as this was requested second
            // and the report rows are ordered by "startTime".
            var queryReportRow2 = reportRows[1];
            AssertReportRowForRequest1Ok(queryReportRow2);
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
        
        private static void AssertReportRowForRequest1Ok(QueryReportLine queryReportRow)
        {
            Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.DataSetId);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow.DataSetVersionId);
            Assert.Equal("1.2.0", queryReportRow.DataSetVersion);
            Assert.Equal("1.*", queryReportRow.RequestedDataSetVersion);
            Assert.Equal("Data Set 1", queryReportRow.DataSetTitle);
            Assert.Equal(DateTime.Parse("2025-02-24T03:07:44.850Z"), queryReportRow.StartTime);
            Assert.Equal("""["Filters","Indicators","Locations","TimePeriods"]""", queryReportRow.Types);
            
            Assert.NotNull(queryReportRow.PreviewToken);
            var previewToken = JsonSerializer.Deserialize<PreviewToken>(
                queryReportRow.PreviewToken, 
                options: new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            Assert.NotNull(previewToken);
            Assert.Equal("Preview token for dashboard testing", previewToken.Label);
            Assert.Equal(DateTime.Parse("2025-02-23T11:02:44.850Z"), previewToken.Created);
            Assert.Equal(DateTime.Parse("2025-02-24T11:02:44.850Z"), previewToken.Expiry);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), previewToken.DataSetVersionId);
        }
        
        private static void AssertReportRowForRequest2Ok(QueryReportLine queryReportRow)
        {
            Assert.Equal(Guid.Parse("01d29401-7274-a871-a8db-d4bc4e98c324"), queryReportRow.DataSetId);
            Assert.Equal(Guid.Parse("01d29401-7974-1276-a06b-b28a6a5385c6"), queryReportRow.DataSetVersionId);
            Assert.Equal("1.2.0", queryReportRow.DataSetVersion);
            Assert.Equal("1.*", queryReportRow.RequestedDataSetVersion);
            Assert.Equal("Data Set 1", queryReportRow.DataSetTitle);
            Assert.Equal(DateTime.Parse("2025-02-24T02:07:44.850Z"), queryReportRow.StartTime);
            Assert.Equal("""["Filters","Locations","TimePeriods"]""", queryReportRow.Types);
            Assert.Null(queryReportRow.PreviewToken);
        }
    }

    private PublicApiGetMetaProcessor BuildService(
        TestAnalyticsPathResolver pathResolver)
    {
        return new PublicApiGetMetaProcessor(
            duckDbConnection: new DuckDbConnection(),
            pathResolver: pathResolver,
            Mock.Of<ILogger<PublicApiGetMetaProcessor>>());
    }

    private void SetupQueryRequest(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicApiGetMetaDirectoryPath());

        var sourceFilePath = Path.Combine(_queryResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicApiGetMetaDirectoryPath(), filename));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record QueryReportLine(
        Guid DataSetId,
        string DataSetTitle,
        string DataSetVersion,
        Guid DataSetVersionId,
        string PreviewToken,
        string RequestedDataSetVersion,
        DateTime StartTime,
        string Types);

    // ReSharper disable once ClassNeverInstantiated.Local
    private record PreviewToken(
        string? Label,
        DateTime Created,
        DateTime Expiry,
        Guid DataSetVersionId);
}
