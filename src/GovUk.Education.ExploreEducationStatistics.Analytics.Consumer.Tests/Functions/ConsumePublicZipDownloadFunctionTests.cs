using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using InterpolatedSql.Dapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Functions;

public abstract class ConsumePublicZipDownloadsFunctionTests : IDisposable
{
    private readonly string _zipDownloadsResourcesPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetDirectoryPath(),
        "Resources",
        "PublicZipDownloads");

    private readonly TestAnalyticsPathResolver _pathResolver = new();

    public void Dispose()
    {
        if (Directory.Exists(_pathResolver.BasePath()))
        {
            Directory.Delete(_pathResolver.BasePath(), recursive: true);
        }
    }

    public class FunctionTests : ConsumePublicZipDownloadsFunctionTests
    {
        [Fact]
        public async Task NoSourceFolder_NoReportsProduced()
        {
            var function = BuildFunction();
            await function.Run(new TimerInfo());
            
            Assert.False(Directory.Exists(_pathResolver.PublicZipDownloadsProcessingDirectoryPath()));
            Assert.False(Directory.Exists(_pathResolver.PublicZipDownloadsReportsDirectoryPath()));
        }
        
        [Fact]
        public async Task EmptyPublicZipDownloadsDirectory_NoReportsProduced()
        {
            Directory.CreateDirectory(_pathResolver.PublicZipDownloadsDirectoryPath());

            var function = BuildFunction();
            await function.Run(new TimerInfo());
            
            // Check that as there were no files to process, no working directories were
            // created as a result.
            Assert.False(Directory.Exists(_pathResolver.PublicZipDownloadsProcessingDirectoryPath()));
            Assert.False(Directory.Exists(_pathResolver.PublicZipDownloadsReportsDirectoryPath()));
        }

        [Fact]
        public async Task SingleSourceQuery_ProducesOneReportRow()
        {
            SetupZipDownloadRequest("ZipDownloadRequest_AllFiles.json");

            var function = BuildFunction();
            await function.Run(new TimerInfo());

            Assert.False(Directory.Exists(_pathResolver.PublicZipDownloadsProcessingDirectoryPath()));
            Assert.True(Directory.Exists(_pathResolver.PublicZipDownloadsReportsDirectoryPath()));

            var reports = Directory.GetFiles(_pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var reportFilename = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", reportFilename);
            
            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadZipDownloadReport(duckDbConnection, reportFilename);
            var zipDownloadReportRow = Assert.Single(zipDownloadReportRows);

            Assert.Equal("publication name", zipDownloadReportRow.PublicationName);
            Assert.Equal(new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"), zipDownloadReportRow.ReleaseVersionId);
            Assert.Equal("release name", zipDownloadReportRow.ReleaseName);
            Assert.Equal("release-label", zipDownloadReportRow.ReleaseLabel);
            Assert.Null(zipDownloadReportRow.SubjectId);
            Assert.Null(zipDownloadReportRow.DataSetName);
            Assert.Equal(1, zipDownloadReportRow.Downloads);
        }

        [Fact]
        public async Task TwoDifferentSourceQueries_ProduceTwoDistinctReportRows()
        {
            SetupZipDownloadRequest("ZipDownloadRequest_AllFiles.json");
            SetupZipDownloadRequest("ZipDownloadRequest_SpecificFiles.json");

            var function = BuildFunction();
            await function.Run(new TimerInfo());

            var reports = Directory.GetFiles(_pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var reportFilename = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", reportFilename);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadZipDownloadReport(duckDbConnection, reportFilename);
            Assert.Equal(2, zipDownloadReportRows.Count);

            // ZipDownloadRequest_SpecificFiles.json
            Assert.Equal("publication name 2", zipDownloadReportRows[0].PublicationName);
            Assert.Equal(new Guid("5ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"), zipDownloadReportRows[0].ReleaseVersionId);
            Assert.Equal("release name 2", zipDownloadReportRows[0].ReleaseName);
            Assert.Equal("release-label-2", zipDownloadReportRows[0].ReleaseLabel);
            Assert.NotNull(zipDownloadReportRows[0].SubjectId);
            Assert.Equal(new Guid("9e3bdced-d289-4017-b93f-23ecfb3c90b9"), zipDownloadReportRows[0].SubjectId);
            Assert.Equal("data set name", zipDownloadReportRows[0].DataSetName);
            Assert.Equal(1, zipDownloadReportRows[0].Downloads);

            // ZipDownloadRequest_AllFiles.json
            Assert.Equal("publication name", zipDownloadReportRows[1].PublicationName);
            Assert.Equal(new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"), zipDownloadReportRows[1].ReleaseVersionId);
            Assert.Equal("release name", zipDownloadReportRows[1].ReleaseName);
            Assert.Equal("release-label", zipDownloadReportRows[1].ReleaseLabel);
            Assert.Null(zipDownloadReportRows[1].SubjectId);
            Assert.Null(zipDownloadReportRows[1].DataSetName);
            Assert.Equal(1, zipDownloadReportRows[1].Downloads);
        }

        [Fact]
        public async Task MultipleRequestsForSameReleaseVersion_ProduceSingleRow()
        {
            SetupZipDownloadRequest("ZipDownloadRequest_AllFiles.json");
            SetupZipDownloadRequest("ZipDownloadRequest_AllFiles_Copy.json");

            var function = BuildFunction();
            await function.Run(new TimerInfo());

            var reports = Directory.GetFiles(_pathResolver.PublicZipDownloadsReportsDirectoryPath());
            var reportFilename = Assert.Single(reports);

            Assert.EndsWith("public-zip-downloads.parquet", reportFilename);

            var duckDbConnection = new DuckDbConnection();
            duckDbConnection.Open();

            var zipDownloadReportRows = await ReadZipDownloadReport(duckDbConnection, reportFilename);
            var zipDownloadReportRow = Assert.Single(zipDownloadReportRows);

            Assert.Equal("publication name", zipDownloadReportRow.PublicationName);
            Assert.Equal(new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"), zipDownloadReportRow.ReleaseVersionId);
            Assert.Equal("release name", zipDownloadReportRow.ReleaseName);
            Assert.Equal("release-label", zipDownloadReportRow.ReleaseLabel);
            Assert.Null(zipDownloadReportRow.SubjectId);
            Assert.Null(zipDownloadReportRow.DataSetName);
            Assert.Equal(2, zipDownloadReportRow.Downloads);
        }

        private static async Task<List<ZipDownloadReportRow>> ReadZipDownloadReport(
            DuckDbConnection duckDbConnection,
            string zipDownloadReportFile)
        {
            var queryReportRows = (await duckDbConnection
                    .SqlBuilder($"SELECT * FROM read_parquet('{zipDownloadReportFile:raw}')")
                    .QueryAsync<ZipDownloadReportRow>())
                .ToList();
            return queryReportRows;
        }
    }

    private ConsumePublicZipDownloadsFunction BuildFunction()
    {
        return new(
            duckDbConnection: new DuckDbConnection(),
            pathResolver: _pathResolver,
            Mock.Of<ILogger<ConsumePublicZipDownloadsFunction>>());
    }
    
    private void SetupZipDownloadRequest(string filename)
    {
        Directory.CreateDirectory(_pathResolver.PublicZipDownloadsDirectoryPath());

        var sourceFilePath = Path.Combine(_zipDownloadsResourcesPath, filename);
        File.Copy(sourceFilePath, Path.Combine(_pathResolver.PublicZipDownloadsDirectoryPath(), filename));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record ZipDownloadReportRow(
        string ZipDownloadHash,
        string PublicationName,
        Guid ReleaseVersionId,
        string ReleaseName,
        string ReleaseLabel,
        Guid? SubjectId,
        string? DataSetName,
        int Downloads);
}
