using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using InterpolatedSql.Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class TableToolDownloadsProcessorTests : ProcessorTestsBase
{
    protected override string ResourcesPath =>
        Path.Combine(
            Assembly.GetExecutingAssembly().GetDirectoryPath(),
            "Resources",
            "DataApi",
            "TableToolDownloads",
            "FromTableToolBuilderPage"
        );

    public class ProcessTests : TableToolDownloadsProcessorTests
    {
        [Fact]
        public async Task ProcessingRequestFile_ProducesReportRow()
        {
            var service = BuildService();
            SetupRequestFile(service, "Example1.json");

            await service.Process();

            // The root processing folder is safe to leave behind.
            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));

            // The temporary processing folder that was set up for this run of the processor
            // should have been cleared away.
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));
            Assert.True(Directory.Exists(service.ReportsDirectory));

            var reports = Directory.GetFiles(service.ReportsDirectory);
            var parquetFile = Assert.Single(reports);

            Assert.EndsWith(TableToolDownloadsProcessor.ReportFileSuffix, parquetFile);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var csvDownloadReportRows = await ReadReport(duckDbConnection, parquetFile);

            var captureTableToolDownloadLine = Assert.Single(csvDownloadReportRows);

            await AssertReportRow(
                captureTableToolDownloadLine,
                "Example1.json",
                1,
                expectedHash: "a9e223bcda0b5a3832bcec68025d7bd8"
            );
        }

        [Fact]
        public async Task TwoDifferentSourceQueries_ProduceTwoDistinctReportRows()
        {
            var service = BuildService();
            SetupRequestFile(service, "Example1.json");
            SetupRequestFile(service, "Example2.json");

            await service.Process();

            // The root processing folder is safe to leave behind.
            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));

            // The temporary processing folder that was set up for this run of the processor
            // should have been cleared away.
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));
            Assert.True(Directory.Exists(service.ReportsDirectory));

            var reports = Directory.GetFiles(service.ReportsDirectory);
            var parquetFile = Assert.Single(reports);

            Assert.EndsWith(TableToolDownloadsProcessor.ReportFileSuffix, parquetFile);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var csvDownloadReportRows = await ReadReport(duckDbConnection, parquetFile);

            Assert.Equal(2, csvDownloadReportRows.Count);

            await AssertReportRow(
                csvDownloadReportRows[0],
                "Example1.json",
                1,
                expectedHash: "a9e223bcda0b5a3832bcec68025d7bd8"
            );

            await AssertReportRow(
                csvDownloadReportRows[1],
                "Example2.json",
                1,
                expectedHash: "93d5d86bf80c7ba877c6c9a5f5f3bac5"
            );
        }

        [Fact]
        public async Task MultipleRequestFilesForSameCsvFile_ProduceSingleReportRow()
        {
            var service = BuildService();
            SetupRequestFile(service, "Example1.json");
            SetupRequestFile(service, "Example1_copy.json");

            await service.Process();

            var reports = Directory.GetFiles(service.ReportsDirectory);
            var parquetFile = Assert.Single(reports);

            Assert.EndsWith(TableToolDownloadsProcessor.ReportFileSuffix, parquetFile);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var csvDownloadReportRows = await ReadReport(duckDbConnection, parquetFile);

            var csvDownloadReportRow = Assert.Single(csvDownloadReportRows);

            await AssertReportRow(
                csvDownloadReportRow,
                "Example1.json",
                2,
                expectedHash: "a9e223bcda0b5a3832bcec68025d7bd8"
            );
        }

        private static async Task<List<CaptureTableToolDownloadLine>> ReadReport(
            DuckDbConnection duckDbConnection,
            string reportFile
        )
        {
            return (
                await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{reportFile:raw}')")
                    .QueryAsync<CaptureTableToolDownloadLine>()
            )
                .OrderBy(row => row.DataSetName)
                .ToList();
        }
    }

    private TableToolDownloadsProcessor BuildService()
    {
        return new TableToolDownloadsProcessor(pathResolver: PathResolver, workflow: Workflow);
    }

    private async Task AssertReportRow(
        CaptureTableToolDownloadLine row,
        string jsonFileName,
        int numRequests,
        string? expectedHash = null
    )
    {
        var jsonText = await File.ReadAllTextAsync(Path.Combine(ResourcesPath, jsonFileName));

        var request = JsonConvert.DeserializeObject<CaptureTableToolDownloadCallDto>(jsonText);
        Assert.NotNull(request);

        Assert.Equal(request.ReleaseVersionId, row.ReleaseVersionId);
        Assert.Equal(request.PublicationName, row.PublicationName);
        Assert.Equal(request.ReleasePeriodAndLabel, row.ReleasePeriodAndLabel);
        Assert.Equal(request.SubjectId, row.SubjectId);
        Assert.Equal(request.DataSetName, row.DataSetName);
        Assert.Equal(request.DownloadFormat, row.DownloadFormat);
        Assert.Equal(request.QueryToString, row.Query);

        if (expectedHash is not null)
        {
            Assert.Equal(expectedHash, row.TableToolDownloadHash);
        }
        Assert.Equal(numRequests, row.Downloads);
    }

    public record CaptureTableToolDownloadCallDto
    {
        public required Guid ReleaseVersionId { get; init; }
        public required string PublicationName { get; init; }
        public required string ReleasePeriodAndLabel { get; init; }
        public required Guid SubjectId { get; init; }
        public required string DataSetName { get; init; }
        public required TableDownloadFormat DownloadFormat { get; init; }
        public required FullTableQuery Query { get; init; }
        public string QueryToString =>
            JsonConvert.SerializeObject(
                Query,
                new JsonSerializerSettings() { ContractResolver = new CustomContractResolver() }
            );

        private class CustomContractResolver : CamelCasePropertyNamesContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(
                Type type,
                MemberSerialization memberSerialization
            ) =>
                base
                    // Yield the properties to be serialised in alphabetical order
                    .CreateProperties(type, memberSerialization)
                    .OrderBy(p => p.Order ?? int.MaxValue)
                    .ThenBy(p => p.PropertyName)
                    .ToList();
        }
    }

    public enum TableDownloadFormat
    {
        CSV,
        ODS,
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record CaptureTableToolDownloadLine
    {
        public required Guid ReleaseVersionId { get; init; }
        public required string PublicationName { get; init; }
        public required string ReleasePeriodAndLabel { get; init; }
        public required Guid SubjectId { get; init; }
        public required string DataSetName { get; init; }
        public required TableDownloadFormat DownloadFormat { get; init; }
        public required string TableToolDownloadHash { get; init; }
        public required string Query { get; init; }
        public int Downloads { get; init; }
    }
}
