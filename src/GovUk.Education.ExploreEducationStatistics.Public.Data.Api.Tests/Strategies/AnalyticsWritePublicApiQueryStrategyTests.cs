using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Strategies;

public abstract class AnalyticsWritePublicApiQueryStrategyTests
{
    public class ReportTests : AnalyticsWritePublicApiQueryStrategyTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsWritePublicApiQueryStrategyTests)}.{nameof(ReportTests)}";

        [Fact]
        public async Task CallWrittenSuccessfully_WithCoreQueryDetails()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            var service = BuildStrategy(pathResolver);

            await service.Report(new CaptureDataSetVersionQueryRequest(
                DataSetId: new Guid("acb97c97-89c9-4b74-88e7-39c27f6bab63"),
                DataSetVersionId: new Guid("0da7c640-80a8-44e2-8028-fc529bcedcb1"),
                DataSetVersion: "2.3.1",
                DataSetTitle: "Data Set Title",
                PreviewToken: null,
                RequestedDataSetVersion: null,
                Query: DataSetQueryRequestTestData.NestedQuery1,
                ResultsCount: 55,
                TotalRowsCount: 5100,
                StartTime: DateTime.Parse("2025-02-20T12:00:00.000Z"),
                EndTime: DateTime.Parse("2025-02-20T12:00:10.234Z")), default);

            var files = Directory
                .GetFiles(pathResolver.BuildOutputDirectory(AnalyticsWritePublicApiQueryStrategy.OutputSubPaths))
                .Order()
                .ToList();

            var file = Assert.Single(files);
            var fileContents = await File.ReadAllTextAsync(file);

            Snapshot.Match(
                currentResult: fileContents,
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithCoreQueryDetails)}");
        }

        [Fact]
        public async Task CallWrittenSuccessfully_WithPreviewTokenAndRequestedDataSetVersion()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            var service = BuildStrategy(pathResolver);

            await service.Report(new CaptureDataSetVersionQueryRequest(
                DataSetId: new Guid("acb97c97-89c9-4b74-88e7-39c27f6bab63"),
                DataSetVersionId: new Guid("0da7c640-80a8-44e2-8028-fc529bcedcb1"),
                DataSetVersion: "2.3.1",
                DataSetTitle: "Data Set Title",
                new PreviewTokenRequest(
                    Label: "Preview token content",
                    DataSetVersionId: new Guid("01d29401-7974-1276-a06b-b28a6a5385c6"),
                    Created: DateTime.Parse("2025-02-23T11:02:44.850Z"),
                    Expiry: DateTime.Parse("2025-02-24T11:02:44.850Z")),
                RequestedDataSetVersion: "1.*",
                Query: DataSetQueryRequestTestData.NestedQuery1,
                ResultsCount: 55,
                TotalRowsCount: 5100,
                StartTime: DateTime.Parse("2025-02-20T12:00:00.000Z"),
                EndTime: DateTime.Parse("2025-02-20T12:00:10.234Z")), default);

            var files = Directory
                .GetFiles(pathResolver.BuildOutputDirectory(AnalyticsWritePublicApiQueryStrategy.OutputSubPaths))
                .Order()
                .ToList();

            var file = Assert.Single(files);
            var fileContents = await File.ReadAllTextAsync(file);

            Snapshot.Match(
                currentResult: fileContents,
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithPreviewTokenAndRequestedDataSetVersion)}");
        }

        private static AnalyticsWritePublicApiQueryStrategy BuildStrategy(
            IAnalyticsPathResolver pathResolver,
            DateTimeProvider? dateTimeProvider = null)
        {
            return new AnalyticsWritePublicApiQueryStrategy(
                pathResolver,
                new CommonAnalyticsWriteStrategyWorkflow<CaptureDataSetVersionQueryRequest>(
                    dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
                    Mock.Of<ILogger<CommonAnalyticsWriteStrategyWorkflow<CaptureDataSetVersionQueryRequest>>>()));
        }
    }
}
