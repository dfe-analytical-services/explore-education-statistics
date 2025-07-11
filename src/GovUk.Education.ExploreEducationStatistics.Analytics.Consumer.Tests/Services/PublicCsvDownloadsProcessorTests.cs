using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicCsvDownloadsProcessorTests : ProcessorTestsBase
{
    protected override string ResourcesPath => Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicCsvDownloads");

    public class ProcessTests : PublicCsvDownloadsProcessorTests
    {
        [Fact]
        public async Task TwoDifferentSourceQueries_ProduceTwoDistinctReportRows()
        {
            var service = BuildService();
            SetupRequestFile(service, "CsvDownloadRequestFile1.json");
            SetupRequestFile(service, "CsvDownloadRequestFile2.json");

            await service.Process();

            // The root processing folder is safe to leave behind.
            Assert.True(Directory.Exists(ProcessingDirectoryPath(service)));
            
            // The temporary processing folder that was set up for this run of the processor
            // should have been cleared away.
            Assert.False(Directory.Exists(TemporaryProcessingDirectoryPath(service)));
            Assert.True(Directory.Exists(service.ReportsDirectory));
            
            var reports = Directory.GetFiles(service.ReportsDirectory);
            var csvDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-csv-downloads.parquet", csvDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var csvDownloadReportRows = await ReadReport(duckDbConnection, csvDownloadsReport);

            Assert.Equal(2, csvDownloadReportRows.Count);

            await AssertReportRow(
                csvDownloadReportRows[0],
                "CsvDownloadRequestFile1.json",
                1);

            await AssertReportRow(
                csvDownloadReportRows[1],
                "CsvDownloadRequestFile2.json",
                1);
        }

        [Fact]
        public async Task MultipleRequestFilesForSameCsvFile_ProduceSingleReportRow()
        {
            var service = BuildService();
            SetupRequestFile(service, "CsvDownloadRequestFile1.json");
            SetupRequestFile(service, "CsvDownloadRequestFile1_Copy.json");

            await service.Process();

            var reports = Directory.GetFiles(service.ReportsDirectory);
            var csvDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-csv-downloads.parquet", csvDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var csvDownloadReportRows = await ReadReport(duckDbConnection, csvDownloadsReport);

            var csvDownloadReportRow = Assert.Single(csvDownloadReportRows);

            await AssertReportRow(
                csvDownloadReportRow,
                "CsvDownloadRequestFile1.json",
                2);
        }

        private static async Task<List<CsvDownloadReportLine>> ReadReport(DuckDbConnection duckDbConnection,
            string reportFile)
        {
            return (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{reportFile:raw}')")
                    .QueryAsync<CsvDownloadReportLine>())
                .OrderBy(row => row.DataSetTitle)
                .ToList();
        }
    }

    private PublicCsvDownloadsProcessor BuildService()
    {
        return new PublicCsvDownloadsProcessor(
            pathResolver: PathResolver,
            workflow: Workflow);
    }

    private async Task AssertReportRow(
        CsvDownloadReportLine row,
        string jsonFileName,
        int numRequests)
    {
        var jsonText = await File.ReadAllTextAsync(Path.Combine(ResourcesPath, jsonFileName));

        var request = JsonConvert.DeserializeObject<CaptureCsvDownloadRequest>(jsonText);
        Assert.NotNull(request);

        Assert.Equal(request.PublicationName, row.PublicationName);
        Assert.Equal(request.ReleaseVersionId, row.ReleaseVersionId);
        Assert.Equal(request.ReleaseName, row.ReleaseName);
        Assert.Equal(request.ReleaseLabel, row.ReleaseLabel);
        Assert.Equal(request.SubjectId, row.SubjectId);
        Assert.Equal(request.DataSetTitle, row.DataSetTitle);

        // Generate expected CsvDownloadHash
        var strToHash = $"{request.SubjectId.ToString().ToLower()}{request.ReleaseVersionId.ToString().ToLower()}";
        var bytesToHash = Encoding.UTF8.GetBytes(strToHash);
        var hash = MD5.Create().ComputeHash(bytesToHash);
        var hashSb = new StringBuilder();
        hash.ForEach(b => hashSb.Append(b.ToString("x2")));

        Assert.Equal(hashSb.ToString(), row.CsvDownloadHash);
        Assert.Equal(numRequests, row.Downloads);
    }

    public record CaptureCsvDownloadRequest(
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string? ReleaseLabel,
        Guid SubjectId,
        string DataSetTitle);

    // ReSharper disable once ClassNeverInstantiated.Local
    private record CsvDownloadReportLine(
        string CsvDownloadHash,
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string? ReleaseLabel,
        Guid SubjectId,
        string DataSetTitle,
        int Downloads);
}
