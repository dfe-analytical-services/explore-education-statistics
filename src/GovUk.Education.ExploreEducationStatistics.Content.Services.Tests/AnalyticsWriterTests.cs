using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class AnalyticsWriterTests : IDisposable
{
    private readonly TestAnalyticsPathResolver _analyticsPathResolver = new();

    private const string SnapshotPrefix = nameof(AnalyticsWriterTests); // @MarkFix

    public void Dispose() // @MarkFix
    {
        if (Directory.Exists(_analyticsPathResolver.BasePath()))
        {
            Directory.Delete(_analyticsPathResolver.BasePath(), recursive: true);
        }
    }

    [Fact]
    public async Task TwoZipDownloads_Success() // @MarkFix should be moved to the strategy?
    {
        var strategyMock = new Mock<IAnalyticsWriteStrategy>(MockBehavior.Strict);

        strategyMock
            .Setup(m =>
                m.CanHandle(It.IsAny<CaptureZipDownloadRequest>()))
            .Returns(true);

        strategyMock
            .Setup(m =>
                m.Report(It.IsAny<CaptureZipDownloadRequest>())) // @MarkFix
            .Returns(Task.CompletedTask);

        var service = BuildService([strategyMock.Object]);

        await service.Report(new CaptureZipDownloadRequest(
            PublicationName: "Publication name",
            ReleaseVersionId: new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"),
            ReleaseName: "Release name",
            ReleaseLabel: "Release label",
            SubjectId: new Guid("9e3bdced-d289-4017-b93f-23ecfb3c90b9"),
            DataSetName: "Data set name"));

        await service.Report(new CaptureZipDownloadRequest(
            PublicationName: "Publication name",
            ReleaseVersionId: new Guid("4ed767c7-79e6-4bd4-a0d1-8c9b7f4bbfaa"),
            ReleaseName: "Release name",
            ReleaseLabel: "Release label",
            SubjectId: null,
            DataSetName: null));

        strategyMock.Verify(m => m.Report(It.IsAny<CaptureZipDownloadRequest>()),
            Times.Exactly(2));

        //var files = Directory // @MarkFix
        //    .GetFiles(_analyticsPathResolver.PublicZipDownloadsDirectoryPath())
        //    .Order()
        //    .ToList();

        //Assert.Equal(2, files.Count);

        //var file1Contents = await File.ReadAllTextAsync(files[0]);
        //Snapshot.Match(
        //    currentResult: file1Contents,
        //    snapshotName: $"{SnapshotPrefix}.{nameof(TwoZipDownloads_Success)}.ZipDownload1");

        //var file2Contents = await File.ReadAllTextAsync(files[1]);
        //Snapshot.Match(
        //    currentResult: file2Contents,
        //    snapshotName: $"{SnapshotPrefix}.{nameof(TwoZipDownloads_Success)}.ZipDownload2");
    }

    private static AnalyticsWriter BuildService(List<IAnalyticsWriteStrategy> strategies)
    {
        return new AnalyticsWriter(
            new List<IAnalyticsWriteStrategy>(strategies));
    }
}
