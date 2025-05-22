using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class PublicZipDownloadsProcessorTests
{
    private readonly string _resourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicZipDownloads");

    public class ProcessTests : PublicZipDownloadsProcessorTests
    {
        [Fact]
        public async Task ProcessorUsesWorkflow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "ZipDownloadRequestFile_NoSubjectId.json");

            var workflowActorBuilder = new WorkflowActorMockBuilder<PublicZipDownloadsProcessor>();
            
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
        public async Task SingleRequestFileNoSubjectId_ProducesOneReportRow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            SetupRequestFile(pathResolver, "ZipDownloadRequestFile_NoSubjectId.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(ProcessingDirectoryPath(pathResolver)));
            Assert.True(Directory.Exists(pathResolver.PublicZipDownloadsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var zipDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", zipDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadReport(duckDbConnection, zipDownloadsReport);

            // Check that the single recorded zip download has resulted in a
            // single line in the report and the values match the
            // values from the original JSON file and the calculated fields
            // match the expected values also.
            var zipDownloadReportRow = Assert.Single(zipDownloadReportRows);

            await AssertReportRow(
                zipDownloadReportRow,
                "ZipDownloadRequestFile_NoSubjectId.json",
                1);
        }

        [Fact]
        public async Task TwoDifferentSourceQueries_ProduceTwoDistinctReportRows()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            SetupRequestFile(pathResolver, "ZipDownloadRequestFile_NoSubjectId.json");
            SetupRequestFile(pathResolver, "ZipDownloadRequestFile_WithSubjectId.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var zipDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", zipDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadReport(duckDbConnection, zipDownloadsReport);

            Assert.Equal(2, zipDownloadReportRows.Count);

            await AssertReportRow(
                zipDownloadReportRows[0],
                "ZipDownloadRequestFile_NoSubjectId.json",
                1);

            await AssertReportRow(
                zipDownloadReportRows[1],
                "ZipDownloadRequestFile_WithSubjectId.json",
                1);
        }

        [Fact]
        public async Task MultipleRequestFilesForSameZipFile_ProduceSingleReportRow()
        {
            using var pathResolver = new TestAnalyticsPathResolver();

            SetupRequestFile(pathResolver, "ZipDownloadRequestFile_WithSubjectId.json");
            SetupRequestFile(pathResolver, "ZipDownloadRequestFile_WithSubjectId_Copy.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var zipDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", zipDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadReport(duckDbConnection, zipDownloadsReport);

            var zipDownloadReportRow = Assert.Single(zipDownloadReportRows);

            await AssertReportRow(
                zipDownloadReportRow,
                "ZipDownloadRequestFile_WithSubjectId.json",
                2);
        }

        private static async Task<List<ZipDownloadReportLine>> ReadReport(DuckDbConnection duckDbConnection, string reportFile)
        {
            return (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{reportFile:raw}')")
                    .QueryAsync<ZipDownloadReportLine>())
                .OrderBy(row => row.DataSetTitle)
                .ToList();
        }
    }

    private PublicZipDownloadsProcessor BuildService(
        TestAnalyticsPathResolver pathResolver,
        IWorkflowActor<PublicZipDownloadsProcessor>? workflowActor = null)
    {
        return new PublicZipDownloadsProcessor(
            pathResolver: pathResolver,
            Mock.Of<ILogger<PublicZipDownloadsProcessor>>(),
            workflowActor: workflowActor);
    }

    private void SetupRequestFile(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicZipDownloadsDirectoryPath());

        var sourceFilePath = Path.Combine(_resourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicZipDownloadsDirectoryPath(), filename));
    }

    private async Task AssertReportRow(
        ZipDownloadReportLine row,
        string jsonFileName,
        int numRequests)
    {
        var jsonText = await File.ReadAllTextAsync(Path.Combine(_resourcesPath, jsonFileName));

        var request = JsonConvert.DeserializeObject<CaptureZipDownloadRequest>(jsonText);
        Assert.NotNull(request);

        Assert.Equal(request.PublicationName, row.PublicationName);
        Assert.Equal(request.ReleaseVersionId, row.ReleaseVersionId);
        Assert.Equal(request.ReleaseName, row.ReleaseName);
        Assert.Equal(request.ReleaseLabel, row.ReleaseLabel);
        Assert.Equal(request.SubjectId, row.SubjectId);
        Assert.Equal(request.DataSetTitle, row.DataSetTitle);
        Assert.Equal(request.FromPage, row.FromPage);

        // Generate expected ZipDownloadHash
        var subjectIdStr = request.SubjectId == null ? "" : request.SubjectId.ToString()!.ToLower();
        var strToHash = $"{subjectIdStr}{request.ReleaseVersionId.ToString().ToLower()}";
        var bytesToHash = Encoding.UTF8.GetBytes(strToHash);
        var hash = MD5.Create().ComputeHash(bytesToHash);
        var hashSb = new StringBuilder();
        hash.ForEach(b => hashSb.Append(b.ToString("x2")));

        Assert.Equal(hashSb.ToString(), row.ZipDownloadHash);
        Assert.Equal(numRequests, row.Downloads);
    }
    
    private static string ProcessingDirectoryPath(TestAnalyticsPathResolver pathResolver)
    {
        return Path.Combine(pathResolver.PublicZipDownloadsDirectoryPath(), "processing");
    }

    public record CaptureZipDownloadRequest(
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string? ReleaseLabel,
        string FromPage,
        Guid? SubjectId = null,
        string? DataSetTitle = null);

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ZipDownloadReportLine(
        string ZipDownloadHash,
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string ReleaseLabel,
        Guid? SubjectId,
        string? DataSetTitle,
        string FromPage,
        int Downloads);
}
