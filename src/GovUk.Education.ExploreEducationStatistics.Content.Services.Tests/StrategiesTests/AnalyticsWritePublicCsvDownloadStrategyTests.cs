using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.StrategiesTests;

public class AnalyticsWritePublicCsvDownloadStrategyTests
{
    public class ReportTests : AnalyticsWritePublicCsvDownloadStrategyTests
    {
        private const string SnapshotPrefix =
            $"{nameof(AnalyticsWritePublicCsvDownloadStrategyTests)}.{nameof(ReportTests)}";

        [Fact]
        public async Task ProduceTwoRequestFiles_Success()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            var strategy = BuildStrategy(
                pathResolver: pathResolver);

            var releaseVersionId1 = Guid.Parse("5d3c0aec-c147-48ce-ae26-ef765ffa4a5b");
            var subjectId1 = Guid.Parse("714b997f-3d7c-4c02-843b-55abf944cb4f");
            await strategy.Report(new CaptureCsvDownloadRequest(
                    "publication name 1",
                    releaseVersionId1,
                    "release name 1",
                    "release label 1",
                    subjectId1,
                    "data set title 1"),
                default);

            var releaseVersionId2 = Guid.Parse("254d53e4-1194-4285-82bd-d8a3b7c0853d");
            var subjectId2 = Guid.Parse("19b4993d-3d32-4e64-9573-d9baac320857");
            await strategy.Report(new CaptureCsvDownloadRequest(
                    "publication name 2",
                    releaseVersionId2,
                    "release name 2",
                    "release label 2",
                    subjectId2,
                    DataSetTitle: "data set title 2"),
                default);

            var files = Directory.GetFiles(pathResolver.BuildOutputDirectory(AnalyticsWritePublicCsvDownloadStrategy.OutputSubPaths))
                .ToList();

            Assert.Equal(2, files.Count);

            var requestFile1 = Assert.Single(
                files
                    .Where(file => file.Contains(releaseVersionId1.ToString()))
                    .ToList());
            var requestFile1Contents = await File.ReadAllTextAsync(requestFile1);
            Snapshot.Match(
                currentResult: requestFile1Contents,
                snapshotName: $"{SnapshotPrefix}.{nameof(ProduceTwoRequestFiles_Success)}.SubjectId1.snap");

            var requestFile2 = Assert.Single(
                files
                    .Where(file => file.Contains(releaseVersionId2.ToString()))
                    .ToList());
            var requestFile2Contents = await File.ReadAllTextAsync(requestFile2);
            Snapshot.Match(
                currentResult: requestFile2Contents,
                snapshotName: $"{SnapshotPrefix}.{nameof(ProduceTwoRequestFiles_Success)}.SubjectId2.snap");
        }
    }

    private AnalyticsWritePublicCsvDownloadStrategy BuildStrategy(
        IAnalyticsPathResolver pathResolver,
        DateTimeProvider? dateTimeProvider = null)
    {
        return new AnalyticsWritePublicCsvDownloadStrategy(
            pathResolver,
            new CommonAnalyticsWriteStrategyWorkflow<CaptureCsvDownloadRequest>(
                dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
                Mock.Of<ILogger<CommonAnalyticsWriteStrategyWorkflow<CaptureCsvDownloadRequest>>>()));
    }
}
