using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using Moq;
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
        public async Task NoSourceFolder_NoReportProduced()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicZipDownloadsProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicZipDownloadsReportsDirectoryPath()));
        }

        [Fact]
        public async Task NoRequestFilesToConsume_NoReportProduced()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            Directory.CreateDirectory(pathResolver.PublicZipDownloadsDirectoryPath());

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            // Check that as there were no files to process, no working directories were
            // created as a result.
            Assert.False(Directory.Exists(pathResolver.PublicZipDownloadsProcessingDirectoryPath()));
            Assert.False(Directory.Exists(pathResolver.PublicZipDownloadsReportsDirectoryPath()));
        }

        [Fact]
        public async Task SingleRequestFileNoSubjectId_ProducesOneReportRow()
        {
            var pathResolver = new TestAnalyticsPathResolver();
            SetupZipDownloadRequest(pathResolver, "ZipDownloadRequestFile_NoSubjectId.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            Assert.False(Directory.Exists(pathResolver.PublicZipDownloadsProcessingDirectoryPath()));
            Assert.True(Directory.Exists(pathResolver.PublicZipDownloadsReportsDirectoryPath()));

            var reports = Directory.GetFiles(pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var zipDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", zipDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadZipDownloadReport(duckDbConnection, zipDownloadsReport);

            // Check that the single recorded zip download has resulted in a
            // single line in the report and the values match the
            // values from the original JSON file and the calculated fields
            // match the expected values also.
            var zipDownloadReportRow = Assert.Single(zipDownloadReportRows);

            Assert.Equal("2e1ff1faca8870e00a9ec1fab7e58409", zipDownloadReportRow.ZipDownloadHash);
            Assert.Equal("publication name", zipDownloadReportRow.PublicationName);
            Assert.Equal(Guid.Parse("319750f6-4c33-476c-9e6d-3da7a403201d"), zipDownloadReportRow.ReleaseVersionId);
            Assert.Equal("release name", zipDownloadReportRow.ReleaseName);
            Assert.Equal("release label", zipDownloadReportRow.ReleaseLabel);
            Assert.Null(zipDownloadReportRow.SubjectId);
            Assert.Null(zipDownloadReportRow.DataSetName);
            Assert.Equal(1, zipDownloadReportRow.Downloads);
        }

        [Fact]
        public async Task TwoDifferentSourceQueries_ProduceTwoDistinctReportRows()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            SetupZipDownloadRequest(pathResolver, "ZipDownloadRequestFile_NoSubjectId.json");
            SetupZipDownloadRequest(pathResolver, "ZipDownloadRequestFile_WithSubjectId.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var zipDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", zipDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadZipDownloadReport(duckDbConnection, zipDownloadsReport);

            Assert.Equal(2, zipDownloadReportRows.Count);

            Assert.Equal("2e1ff1faca8870e00a9ec1fab7e58409", zipDownloadReportRows[0].ZipDownloadHash);
            Assert.Equal("publication name", zipDownloadReportRows[0].PublicationName);
            Assert.Equal(Guid.Parse("319750f6-4c33-476c-9e6d-3da7a403201d"), zipDownloadReportRows[0].ReleaseVersionId);
            Assert.Equal("release name", zipDownloadReportRows[0].ReleaseName);
            Assert.Equal("release label", zipDownloadReportRows[0].ReleaseLabel);
            Assert.Null(zipDownloadReportRows[0].SubjectId);
            Assert.Null(zipDownloadReportRows[0].DataSetName);
            Assert.Equal(1, zipDownloadReportRows[0].Downloads);

            Assert.Equal("670871b1327feb682dd3516374f35928", zipDownloadReportRows[1].ZipDownloadHash);
            Assert.Equal("publication name 2", zipDownloadReportRows[1].PublicationName);
            Assert.Equal(Guid.Parse("72a6856c-8d7b-4ad9-b533-2066d171d146"), zipDownloadReportRows[1].ReleaseVersionId);
            Assert.Equal("release name 2", zipDownloadReportRows[1].ReleaseName);
            Assert.Equal("release label 2", zipDownloadReportRows[1].ReleaseLabel);
            Assert.Equal(Guid.Parse("66c5eb5f-a85b-4586-bae8-dc3504d3042f"), zipDownloadReportRows[1].SubjectId);
            Assert.Equal("data set name 2", zipDownloadReportRows[1].DataSetName);
            Assert.Equal(1, zipDownloadReportRows[1].Downloads);
        }

        [Fact]
        public async Task MultipleRequestFilesForSameZipFile_ProduceSingleReportRow()
        {
            var pathResolver = new TestAnalyticsPathResolver();

            SetupZipDownloadRequest(pathResolver, "ZipDownloadRequestFile_WithSubjectId.json");
            SetupZipDownloadRequest(pathResolver, "ZipDownloadRequestFile_WithSubjectId_Copy.json");

            var service = BuildService(
                pathResolver: pathResolver);
            await service.Process();

            var reports = Directory.GetFiles(pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var zipDownloadsReport = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", zipDownloadsReport);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadZipDownloadReport(duckDbConnection, zipDownloadsReport);

            var zipDownloadReportRow = Assert.Single(zipDownloadReportRows);

            Assert.Equal("670871b1327feb682dd3516374f35928", zipDownloadReportRow.ZipDownloadHash);
            Assert.Equal("publication name 2", zipDownloadReportRow.PublicationName);
            Assert.Equal(Guid.Parse("72a6856c-8d7b-4ad9-b533-2066d171d146"), zipDownloadReportRow.ReleaseVersionId);
            Assert.Equal("release name 2", zipDownloadReportRow.ReleaseName);
            Assert.Equal("release label 2", zipDownloadReportRow.ReleaseLabel);
            Assert.Equal(Guid.Parse("66c5eb5f-a85b-4586-bae8-dc3504d3042f"), zipDownloadReportRow.SubjectId);
            Assert.Equal("data set name 2", zipDownloadReportRow.DataSetName);
            Assert.Equal(2, zipDownloadReportRow.Downloads);
        }

        private static async Task<List<ZipDownloadReportLine>> ReadZipDownloadReport(DuckDbConnection duckDbConnection, string reportFile)
        {
            return (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{reportFile:raw}')")
                    .QueryAsync<ZipDownloadReportLine>())
                .ToList();
        }
    }

    private PublicZipDownloadsProcessor BuildService(
        TestAnalyticsPathResolver? pathResolver = null)
    {
        return new PublicZipDownloadsProcessor(
            duckDbConnection: new DuckDbConnection(),
            pathResolver: pathResolver ?? new TestAnalyticsPathResolver(),
            Mock.Of<ILogger<PublicZipDownloadsProcessor>>());
    }

    private void SetupZipDownloadRequest(TestAnalyticsPathResolver pathResolver, string filename)
    {
        Directory.CreateDirectory(pathResolver.PublicZipDownloadsDirectoryPath());

        var sourceFilePath = Path.Combine(_resourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(pathResolver.PublicZipDownloadsDirectoryPath(), filename));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ZipDownloadReportLine(
        string ZipDownloadHash,
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string ReleaseLabel,
        Guid? SubjectId,
        string? DataSetName,
        int Downloads);
}
