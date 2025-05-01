using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.StrategiesTests;

public class AnalyticsWritePublicZipDownloadStrategyTests
{
    public class ReportTests : AnalyticsWritePublicZipDownloadStrategyTests
    {
        private const string SnapshotPrefix =
            $"{nameof(AnalyticsWritePublicZipDownloadStrategyTests)}.{nameof(ReportTests)}";

        [Fact]
        public async Task ProduceTwoRequestFiles_Success()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            var strategy = BuildStrategy(
                pathResolver: pathResolver);

            var releaseVersionId1 = Guid.Parse("5d3c0aec-c147-48ce-ae26-ef765ffa4a5b");
            await strategy.Report(new CaptureZipDownloadRequest(
                    "publication name 1",
                    releaseVersionId1,
                    "release name 1",
                    "release label 1"),
                default);

            var releaseVersionId2 = Guid.Parse("254d53e4-1194-4285-82bd-d8a3b7c0853d");
            await strategy.Report(new CaptureZipDownloadRequest(
                    "publication name 2",
                    releaseVersionId2,
                    "release name 2",
                    "release label 2",
                    SubjectId: Guid.Parse("39132b60-d4a0-4b62-befe-ba10cea4b30e"),
                    DataSetTitle: "data set title 2"),
                default);

            var files = Directory.GetFiles(pathResolver.PublicZipDownloadsDirectoryPath())
                .ToList();

            Assert.Equal(2, files.Count);

            var requestFile1 = Assert.Single(
                files
                    .Where(file => file.Contains(releaseVersionId1.ToString()))
                    .ToList());
            var requestFile1Contents = await File.ReadAllTextAsync(requestFile1);
            Snapshot.Match(
                currentResult: requestFile1Contents,
                snapshotName: $"{SnapshotPrefix}.{nameof(ProduceTwoRequestFiles_Success)}.NoSubjectId.snap");

            var requestFile2 = Assert.Single(
                files
                    .Where(file => file.Contains(releaseVersionId2.ToString()))
                    .ToList());
            var requestFile2Contents = await File.ReadAllTextAsync(requestFile2);
            Snapshot.Match(
                currentResult: requestFile2Contents,
                snapshotName: $"{SnapshotPrefix}.{nameof(ProduceTwoRequestFiles_Success)}.WithSubjectId.snap");
        }
    }

    private AnalyticsWritePublicZipDownloadStrategy BuildStrategy(
        IAnalyticsPathResolver pathResolver,
        ILogger<AnalyticsWritePublicZipDownloadStrategy>? logger = null)
    {
        return new AnalyticsWritePublicZipDownloadStrategy(
            pathResolver,
            logger ?? Mock.Of<ILogger<AnalyticsWritePublicZipDownloadStrategy>>());
    }
}
