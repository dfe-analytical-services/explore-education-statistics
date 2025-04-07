using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class AnalyticsWriterTests : IDisposable
{
    private readonly TestAnalyticsPathResolver _analyticsPathResolver = new();

    private const string SnapshotPrefix = nameof(AnalyticsWriterTests);

    public void Dispose()
    {
        if (Directory.Exists(_analyticsPathResolver.BasePath()))
        {
            Directory.Delete(_analyticsPathResolver.BasePath(), recursive: true);
        }
    }

    [Fact]
    public async Task Success()
    {
        var service = BuildService(_analyticsPathResolver);

        await service.ReportReleaseVersionZipDownload(new AnalyticsWriter.CaptureReleaseVersionZipDownloadRequest(
            PublicationName: "Publication name",
            ReleaseVersionId: new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"),
            ReleaseName: "Release name",
            ReleaseLabel: "Release label",
            SubjectId: new Guid("9e3bdced-d289-4017-b93f-23ecfb3c90b9"),
            DataSetName: "Data set name"));

        await service.ReportReleaseVersionZipDownload(new AnalyticsWriter.CaptureReleaseVersionZipDownloadRequest(
            PublicationName: "Publication name",
            ReleaseVersionId: new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"),
            ReleaseName: "Release name",
            ReleaseLabel: "Release label",
            SubjectId: null,
            DataSetName: null));

        var files = Directory
            .GetFiles(_analyticsPathResolver.PublicZipDownloadsDirectoryPath())
            .Order()
            .ToList();

        Assert.Equal(2, files.Count);

        var file1Contents = await File.ReadAllTextAsync(files[0]);
        Snapshot.Match(
            currentResult: file1Contents,
            snapshotName: $"{SnapshotPrefix}.{nameof(Success)}.ZipDownload1");

        var file2Contents = await File.ReadAllTextAsync(files[1]);
        Snapshot.Match(
            currentResult: file2Contents,
            snapshotName: $"{SnapshotPrefix}.{nameof(Success)}.ZipDownload2");
    }

    private static AnalyticsWriter BuildService(IAnalyticsPathResolver pathResolver)
    {
        return new(pathResolver, Mock.Of<ILogger<AnalyticsWriter>>());
    }
}
